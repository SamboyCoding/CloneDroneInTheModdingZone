namespace CDMZ.EventSystem
{
    public class AboutToLoadNextLevelEvent : Event
    {
        public bool IsALegacyLevel { get; }

        private string _legacyLevelName;
        
        public LevelDescription LevelDescription { get; }
        
        public string LevelName => _legacyLevelName ?? LevelDescription.LevelID;

        public AboutToLoadNextLevelEvent(string levelName)
        {
            IsALegacyLevel = true;
            _legacyLevelName = levelName;
        }

        public AboutToLoadNextLevelEvent(LevelDescription ld)
        {
            LevelDescription = ld;
            IsALegacyLevel = false;
        }
    }
}