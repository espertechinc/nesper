using System;

namespace com.espertech.esper.common.client.hook.exception
{
    /// <summary>
    /// Event data for exception handling.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ExceptionHandlerEventArgs : EventArgs
    {
        public ExceptionHandlerContext Context { get; set; }
        public ExceptionHandlerContextUnassociated InboundPoolContext { get; set; }
        public bool IsInboundPoolException => InboundPoolContext != null;
    }
}
