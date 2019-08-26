namespace CDMZ.EventSystem
{
    public class LevelAboutToLoadEvent : Event
    {
        public bool IsALegacyLevel { get; }

        private string _legacyLevelName;
        
        public LevelDescription LevelDescription { get; }
        
        public string LevelName => _legacyLevelName ?? LevelDescription.LevelID;

        public LevelAboutToLoadEvent(string levelName)
        {
            IsALegacyLevel = true;
            _legacyLevelName = levelName;
            CanBeCancelled = false; //TODO Look into making cancellable, or allowing the leveldescription to be changed
        }

        public LevelAboutToLoadEvent(LevelDescription ld)
        {
            LevelDescription = ld;
            IsALegacyLevel = false;
            CanBeCancelled = false; 
        }
    }
}