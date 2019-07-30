namespace CDMZ.EventSystem
{
    public class CharacterSpawnEvent : Event
    {
        public bool IsPlayerSpawn { get; private set; }
        
        public EnemyType enemyType;

        public CharacterSpawnEvent(EnemyType t)
        {
            IsPlayerSpawn = false;
            enemyType = t;
        }
    }
}