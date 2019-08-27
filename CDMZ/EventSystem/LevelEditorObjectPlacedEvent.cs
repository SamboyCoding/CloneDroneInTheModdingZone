namespace CDMZ.EventSystem
{
    public class LevelEditorObjectPlacedEvent : Event
    {
        public ObjectPlacedInLevel PlacedObject { get; }
        
        public LevelEditorObjectPlacedEvent(ObjectPlacedInLevel obj)
        {
            PlacedObject = obj;
        }
    }
}