namespace CDMZ.EventSystem
{
    public class CharacterPreSpawnEvent : Event
    {
        public bool IsPlayerSpawn { get; }
        
        public EnemyType enemyType;

        public CharacterPreSpawnEvent(EnemyType t)
        {
            IsPlayerSpawn = false;
            enemyType = t;
        }
    }
}