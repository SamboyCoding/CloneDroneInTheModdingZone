namespace CDMZ.EventSystem
{
    public class CharacterPostSpawnEvent : Event
    {
        public Character Character { get; }

        public CharacterPostSpawnEvent(Character c)
        {
            Character = c;
            CanBeCancelled = false;
        }
    }
}