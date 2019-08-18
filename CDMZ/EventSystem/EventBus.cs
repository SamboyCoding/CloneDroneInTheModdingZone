using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;

namespace CDMZ.EventSystem
{
    public sealed class EventBus
    {
        public static EventBus Instance { get; private set; }

        private Logger _logger = new Logger("CDMZ|EventBus");

        private Dictionary<Type, List<Action<Event>>> handlers = new Dictionary<Type, List<Action<Event>>>();

        internal EventBus()
        {
            Instance = this;
        }

        public void Register(Type type)
        {
            var annotated = type.GetMethods()
                .Where(m =>
                    m.GetCustomAttributes(true)
                        .Select(a => a.GetType())
                        .Any(t => t == typeof(EventHandlerAttribute))
                );

            foreach (var method in annotated)
            {
                if (method.GetParameters().Length != 1)
                {
                    _logger.Error($"Found an EventHandler-annotated method with an invalid number of parameters: {method.FullDescription()}");
                    _logger.Error("It must take one argument that is a non-abstract subclass of Event");
                    _logger.Error("This event handler will be ignored - so it will NOT receive events.");
                    continue;
                }

                var param = method.GetParameters()[0];
                var eventType = param.ParameterType;
                if (eventType.IsAbstract)
                {
                    _logger.Error($"Found an EventHandler-annotated method that takes an abstract event: {method.FullDescription()}");
                    _logger.Error("It must take one argument that is a non-abstract subclass of Event");
                    _logger.Error("This event handler will be ignored - so it will NOT receive events.");
                    continue;
                }

                if (!typeof(Event).IsAssignableFrom(eventType))
                {
                    _logger.Error($"Found an EventHandler-annotated method that takes a parameter that is not an Event: {method.FullDescription()}");
                    _logger.Error("It must take one argument that is a non-abstract subclass of Event");
                    _logger.Error("This event handler will be ignored - so it will NOT receive events.");
                    continue;
                }

                if (!method.IsStatic)
                {
                    _logger.Error($"Found an EventHandler-annotated method that is not static: {method.FullDescription()}");
                    _logger.Error("It must be static so that it can be invoked.");
                    _logger.Error("This event handler will be ignored - so it will NOT receive events.");
                }

                _logger.Debug($"Found an EventHandler-annotated method {method.DeclaringType?.FullName}::{method.Name} which will be registered with event {eventType.Name}");

                Register(method, eventType);
            }
        }

        internal void Register<T>(Action<T> handler) where T : Event
        {
            var type = typeof(T);

            //Bloody c# generics
            var actionEvent = new Action<Event>(evt => handler((T) Convert.ChangeType(evt, typeof(T))));

            if (handlers.ContainsKey(type))
                handlers[type].Add(actionEvent);
            else
            {
                _logger.Debug($"Creating listener list for event type {type}");
                handlers[type] = new[] {actionEvent}.ToList();
            }
        }

        private void Register(MethodInfo handler, Type type)
        {
            var actionEvent = new Action<Event>(evt => handler.Invoke(null, new[] {Convert.ChangeType(evt, type)}));

            if (handlers.ContainsKey(type))
                handlers[type].Add(actionEvent);
            else
            {
                _logger.Debug($"Creating listener list for event type {type}");
                handlers[type] = new[] {actionEvent}.ToList();
            }
        }

        /// <summary>
        /// Post an event to the bus
        /// </summary>
        /// <param name="evt">The event to post</param>
        /// <returns>True if the event may continue - i.e. it is not cancellable or it was not cancelled - false if it was cancelled and may not continue.</returns>
        public bool Post(Event evt)
        {
            _logger.Debug($"Dispatching an event of type {evt.GetType()}");

            if (!handlers.ContainsKey(evt.GetType())) return true;

            foreach (var action in handlers[evt.GetType()])
            {
                try
                {
                    action.Invoke(evt);
                }
                catch (Exception e)
                {
                    _logger.Exception(e, $"Exception thrown in event handler for event {evt}:");
                }
            }

            return !evt.IsCancelled();
        }
    }
}