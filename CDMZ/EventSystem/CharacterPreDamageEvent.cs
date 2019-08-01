using UnityEngine;

namespace CDMZ.EventSystem
{
    public class PreDamageEvent : Event
    {
        public FirstPersonMover Damager { get; }
        public Character Damagee { get; }
        public MechBodyPart BodyPart { get; }

        public Type DamageType { get; }

        public PreDamageEvent(MeleeImpactArea mia, MechBodyPart part)
        {
            Damager = mia.Owner;
            Damagee = part.GetOwner();
            BodyPart = part;

            switch (mia)
            {
                case ArrowCutArea _:
                    DamageType = Type.BOW;
                    break;
                case NonMovingHammerMeleeArea _: //Includes MovingLaserImpactArea
                    DamageType = Type.ENVIRONMENT_EXPLOSION;
                    break;
                case HammerImpactMeleeArea _:
                    DamageType = Type.HAMMER;
                    break;
                case SwordHitArea _:
                    DamageType = Type.SWORD;
                    break;
                case BladeCutArea _:
                    //Both Arrow and Sword are subclasses of blade, so if we're here it's a saw blade
                    DamageType = Type.ENVIRONMENT_BLADE;
                    break;
                default:
                    new Logger("DamageSource").Debug($"Unknown damage source - {mia} from {Damager} to {Damagee}");
                    DamageType = Type.OTHER;
                    break;
            }
            
            new Logger("DamageSource").Debug($"{DamageType} damage being dealt from {Damager} to {Damagee}");
        }

        public enum Type
        {
            SWORD,
            BOW,
            HAMMER,
            ENVIRONMENT_EXPLOSION,
            ENVIRONMENT_BLADE,
            OTHER
        }
    }
}