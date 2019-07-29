using CDMZ.EventSystem;

namespace CDMZ
{
    public static class EventListener
    {
        public static void Init()
        {
            EventBus.Instance.Register((CharacterSpawnEvent e) =>
            {
                //I don't like hammer bots
                if (e.CharacterBeingSpawned.CharacterType == EnemyType.Hammer1)
                    e.Cancel();
            });
        }
    }
}