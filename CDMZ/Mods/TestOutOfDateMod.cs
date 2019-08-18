namespace CDMZ
{
    public class TestOutOfDateMod : Mod
    {
        public override string GetModName()
        {
            return "Update Test";
        }

        public override string GetModDescription()
        {
            return "An out-of-date mod to test the out-of-date part of the UI";
        }

        public override string GetVersion()
        {
            return "1.0.0";
        }

        public override bool IsUpToDate()
        {
            return false;
        }

        public override string GetLatestVersion()
        {
            return "1.1.0";
        }

        public override void Enable()
        {
            //No-op
        }

        public override void Disable()
        {
            //No-op
        }
    }
}