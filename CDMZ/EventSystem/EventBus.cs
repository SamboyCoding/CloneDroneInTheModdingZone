using System;
using System.Collections.Generic;
using System.Linq;

namespace CDMZ.EventSystem
{
    public class EventBus : Singleton<EventBus>
    {
        private Logger _logger = new Logger("CDMZ|EventBus");
        
        private Dictionary<Type, List<Action<Event>>> handlers = new Dictionary<Type, List<Action<Event>>>();
        
        internal void Register<T>(Action<T> handler) where T : Event
        {
            var type = typeof(T);
            
            //Bloody c# generics
            var actionEvent = new Action<Event>(evt => handler((T) Convert.ChangeType(evt, typeof(T))));
            
            if(handlers.ContainsKey(type))
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
            
            foreach (var action in handlers[evt.GetType()]) action.Invoke(evt);

            return !evt.IsCancelled();
        }
    }
}