using CDMZ.EventSystem;

namespace CDMZ
{
    public static class EventListener
    {
        public static void Init()
        {
            EventBus.Instance.Register((CharacterPreSpawnEvent e) =>
            {
                //I don't like mk1 swords
                if (!e.IsPlayerSpawn && e.enemyType == EnemyType.Swordsman1)
                    e.Cancel();
            });
        }
    }
}