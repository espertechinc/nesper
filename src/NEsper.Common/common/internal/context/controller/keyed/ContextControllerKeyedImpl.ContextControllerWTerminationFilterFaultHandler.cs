using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public partial class ContextControllerKeyedImpl
    {
        public class ContextControllerWTerminationFilterFaultHandler : FilterFaultHandler
        {
            public static readonly FilterFaultHandler INSTANCE = new ContextControllerWTerminationFilterFaultHandler();

            private ContextControllerWTerminationFilterFaultHandler()
            {
            }

            public bool HandleFilterFault(
                EventBean theEvent,
                long version)
            {
                return true;
            }
        }
    }
}