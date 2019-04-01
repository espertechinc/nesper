namespace com.espertech.esperio
{
    /// <summary>
    /// An Adapter takes some external data, converts it into events, and sends it
    /// into the runtime engine.
    /// </summary>
    public interface Adapter
    {
        /// <summary>
        /// Start the sending of events into the runtime egine.
        /// </summary>
        /// <throws>EPException in case of errors processing the events</throws>
        void Start();

        /// <summary>
        /// Pause the sending of events after a Adapter has been started.
        /// </summary>
        /// <throws>EPException if this Adapter has already been stopped</throws>
        void Pause();

        /// <summary>
        /// Resume sending events after the Adapter has been paused.
        /// </summary>
        /// <throws>EPException in case of errors processing the events</throws>
        void Resume();

        /// <summary>
        /// Stop sending events and return the Adapter to the OPENED state, ready to be
        /// started once again.
        /// </summary>
        /// <throws>EPException in case of errors releasing resources</throws>
        void Stop();

        /// <summary>
        /// Dispose the Adapter, stopping the sending of all events and releasing all
        /// the resources, and disallowing any further state changes on the Adapter.
        /// </summary>
        /// <throws>EPException to indicate errors during destroy</throws>
        void Destroy();

        /// <summary>
        /// Get the state of this Adapter.
        /// </summary>
        AdapterState State { get; }
    }
}