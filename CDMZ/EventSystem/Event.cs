using System;

namespace CDMZ.EventSystem
{
    /// <summary>
    /// Class used both to announce something is happening, so that mods can react to this, and to provide the ability to
    /// cancel certain things, such as enemies spawning or the player dying, or modify the data (such as what enemy is
    /// being spawned or what the player died to, in twitch mode)
    /// </summary>
    public abstract class Event
    {
        public bool CanBeCancelled { get; protected set; } = true;

        private bool _cancelled = false;

        /// <summary>
        /// Attempt to cancel this event, if it can be cancelled (this is determined by the event type)
        /// </summary>
        /// <returns>True if the event can be cancelled (in which case it is now cancelled and whatever depends on it won't happen) or false if it cannot be cancelled (in which case, why are you calling this method?)</returns>
        public bool Cancel()
        {
            if (!CanBeCancelled) throw new InvalidOperationException("Attempt to cancel an event that cannot be cancelled.");
            _cancelled = true;
            return true;
        }

        /// <summary>
        /// Determines if this method has been cancelled by another mod. If an event is cancelled, whatever it is broadcasting (e.g. an enemy spawn) won't occur.
        /// </summary>
        /// <returns></returns>
        public bool IsCancelled()
        {
            return _cancelled;
        }
    }
}