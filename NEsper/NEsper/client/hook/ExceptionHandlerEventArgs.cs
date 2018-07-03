using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.espertech.esper.client.hook
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
