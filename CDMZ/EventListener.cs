using CDMZ.EventSystem;

namespace CDMZ
{
    public static class EventListener
    {
        public static void Init()
        {
            EventBus.Instance.Register((CharacterPreSpawnEvent e) =>
            {
                //I don't like mk1 bows
                if (!e.IsPlayerSpawn && e.enemyType == EnemyType.Bowman1)
                    e.Cancel();
            });
            
            EventBus.Instance.Register((PreDamageEvent e) =>
            {
                //Player godmode
                if (e.Damagee.IsMainPlayer())
                    e.Cancel();
            });
        }
    }
}