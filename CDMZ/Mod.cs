namespace CDMZ
{
    public abstract class Mod
    {
        public abstract string GetModName();
        public abstract string GetModDescription();
        public abstract string GetVersion();

        public virtual bool IsUpToDate()
        {
            return true;
        }

        public virtual string GetLatestVersion()
        {
            return GetVersion();
        }
    }
}