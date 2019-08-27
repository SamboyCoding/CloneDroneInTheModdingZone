namespace CDMZ.EventSystem
{
    public class ProjectileCreatedEvent : Event
    {
        public Projectile Projectile { get; }
        public Type ProjectileType { get; }
        
        public ProjectileCreatedEvent(Projectile proj, Type t)
        {
            Projectile = proj;
            ProjectileType = t;
            CanBeCancelled = false;
        }

        public enum Type
        {
            ARROW, SHRAPNEL, FLAME, REPAIR, OTHER
        }
    }
}