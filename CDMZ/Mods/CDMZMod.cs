using CDMZ.EventSystem;

namespace CDMZ
{
    public class CDMZMod : Mod
    {
        public override string GetModName()
        {
            return "CDMZ Core";
        }

        public override string GetModDescription()
        {
            return "Mod loader for Clone Drone in the Danger Zone";
        }

        public override string GetVersion()
        {
            return "1.0.0";
        }

        public override void Enable()
        {
            EventBus.Instance.Register(typeof(EventListener));
        }

        public override void Disable()
        {
            EventBus.Instance.Unregister(typeof(EventListener));
        }
    }
}