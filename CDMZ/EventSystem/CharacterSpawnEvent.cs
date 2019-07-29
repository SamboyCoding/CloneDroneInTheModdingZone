namespace CDMZ.EventSystem
{
    public class CharacterSpawnEvent : Event
    {
        public Character CharacterBeingSpawned { get; private set; }
        public bool IsPlayerSpawn { get; private set; }
        
        public CharacterSpawnEvent(Character c)
        {
            CharacterBeingSpawned = c;

            if (c.IsClone() || c.IsMainPlayer())
            {
                CanBeCancelled = false; //Do not allow mods to prevent the player from spawning.
                IsPlayerSpawn = true;
            }
        }
    }
}