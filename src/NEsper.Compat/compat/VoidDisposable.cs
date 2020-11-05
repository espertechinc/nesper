using System;

namespace com.espertech.esper.compat
{
    /// <summary>
    /// Does nothing
    /// </summary>
    public class VoidDisposable : IDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
