namespace com.espertech.esperio
{
    /// <summary>
    /// An Adapter takes some external data, converts it into events, and sends it
    /// into the runtime engine.
    /// </summary>
    public interface AdapterSPI
    {
        /// <summary>
        /// Gets or sets the engine instance.
        /// </summary>

        EPServiceProvider EPServiceProvider
        {
            get;
            set;
        }
    }
}