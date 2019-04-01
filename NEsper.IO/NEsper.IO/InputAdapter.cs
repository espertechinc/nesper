namespace com.espertech.esperio
{
    /// <summary>
    /// An InputAdapter takes some external data, converts it into events, and sends
    /// it into the runtime engine.
    /// </summary>
    
    public interface InputAdapter : Adapter
    {
    }

    abstract public class InputAdapter_Fields
    {
        /// <summary>
        /// Use for MapMessage events to indicate the event type name.
        /// </summary>
        
        public readonly string ESPERIO_MAP_EVENT_TYPE = typeof(InputAdapter).FullName + "_maptype";
    }
}