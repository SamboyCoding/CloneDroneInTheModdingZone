namespace CDMZ.EventSystem
{
    public class CharacterPreSpawnEvent : Event
    {
        public bool IsPlayerSpawn { get; private set; }
        
        public EnemyType enemyType;

        public CharacterPreSpawnEvent(EnemyType t)
        {
            IsPlayerSpawn = false;
            enemyType = t;
        }
    }
}