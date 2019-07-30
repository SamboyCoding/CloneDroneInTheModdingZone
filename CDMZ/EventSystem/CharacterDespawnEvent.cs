namespace CDMZ.EventSystem
{
    public class CharacterDespawnEvent : Event
    {
        public Character Character { get; }
        
        public CharacterDespawnEvent(Character c)
        {
            CanBeCancelled = false;
            Character = c;
        }
    }
}