namespace CDMZ.EventSystem
{
    public class CharacterDeathEvent : Event
    {
        public Character Character { get; }

        public CharacterDeathEvent(Character c)
        {
            CanBeCancelled = false;
            Character = c;
        }
    }
}