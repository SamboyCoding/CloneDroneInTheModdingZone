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

        /// <summary>
        /// Contains a list of handlers registered as a map of event type to a list of handlers that want that event.
        /// </summary>
        private Dictionary<Type, List<Action<Event>>> _handlers = new Dictionary<Type, List<Action<Event>>>();
        
        /// <summary>
        /// Contains a list of handlers registered as a map of handler method to a tuple of event type and index in that type's handler list.
        /// </summary>
        private Dictionary<MethodInfo, KeyValuePair<Type, int>> _registeredMethods = new Dictionary<MethodInfo, KeyValuePair<Type, int>>();
        
        /// <summary>
        /// Contains a list of handlers registered as a map of defining type to its methods. 
        /// </summary>
        private Dictionary<Type, List<MethodInfo>> _handlerFunctionsByParentClass = new Dictionary<Type, List<MethodInfo>>();
        

        internal EventBus()
        {
            Instance = this;
        }
        
        [Obsolete("Only for ModBot compat, does not support unregistering")]
        internal void Register<T>(Action<T> handler) where T : Event
        {
            var type = typeof(T);

            //Bloody c# generics
            var actionEvent = new Action<Event>(evt => handler((T) Convert.ChangeType(evt, typeof(T))));

            if (_handlers.ContainsKey(type))
                _handlers[type].Add(actionEvent);
            else
            {
                _logger.Debug($"Creating listener list for event type {type}");
                _handlers[type] = new[] {actionEvent}.ToList();
            }
        }

        public void Register(Type type)
        {
            var annotated = type.GetMethods()
                .Where(m =>
                    m.GetCustomAttributes(true)
                        .Select(a => a.GetType())
                        .Any(t => t == typeof(EventHandlerAttribute))
                );
            
            _handlerFunctionsByParentClass[type] = new List<MethodInfo>();

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
                _handlerFunctionsByParentClass[type].Add(method);
            }
        }

        public void Unregister(Type t)
        {
            foreach (var method in _handlerFunctionsByParentClass[t])
            {
                var registrationInfo = _registeredMethods[method];
                
                //Unregister the handler itself
                _logger.Info($"Unregistering a handler for method type {registrationInfo.Key}: {method.DeclaringType}::{method.Name}");
                _handlers[registrationInfo.Key].RemoveAt(registrationInfo.Value);
                
                //Remove it from registered methods
                _registeredMethods.Remove(method);
            }

            //Remove the class from the classes list.
            _handlerFunctionsByParentClass.Remove(t);
        }

        private void Register(MethodInfo handler, Type type)
        {
            var actionEvent = new Action<Event>(evt => handler.Invoke(null, new[] {Convert.ChangeType(evt, type)}));

            if (_handlers.ContainsKey(type))
            {
                _handlers[type].Add(actionEvent);
                var idx = _handlers.Count - 1;
                _registeredMethods.Add(handler, new KeyValuePair<Type, int>(type, idx));
            }
            else
            {
                _logger.Debug($"Creating listener list for event type {type}");
                _handlers[type] = new[] {actionEvent}.ToList();
                _registeredMethods.Add(handler, new KeyValuePair<Type, int>(type, 0));
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

            if (!_handlers.ContainsKey(evt.GetType())) return true;

            foreach (var action in _handlers[evt.GetType()])
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