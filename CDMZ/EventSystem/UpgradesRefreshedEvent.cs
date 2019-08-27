namespace CDMZ.EventSystem
{
    public class UpgradesRefreshedEvent : Event
    {
        public FirstPersonMover FirstPersonMover { get; }

        public UpgradesRefreshedEvent(FirstPersonMover fpm)
        {
            FirstPersonMover = fpm;
            CanBeCancelled = false;
        }
    }
}