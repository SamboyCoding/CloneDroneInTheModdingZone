using CDMZ.EventSystem;

namespace CDMZ
{
    public static class EventListener
    {
        public static void Init()
        {
            EventBus.Instance.Register((CharacterSpawnEvent e) =>
            {
                //I don't like mk1 swords
                if (e.CharacterBeingSpawned.CharacterType == EnemyType.Swordsman1)
                    e.Cancel();
            });
        }
    }
}