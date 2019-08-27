namespace CDMZ.EventSystem
{
    public class UpgradesAboutToBeRefreshedEvent : Event
    {
        public FirstPersonMover FirstPersonMover { get; }

        public UpgradesAboutToBeRefreshedEvent(FirstPersonMover fpm)
        {
            FirstPersonMover = fpm;
            CanBeCancelled = false;
        }
    }
}