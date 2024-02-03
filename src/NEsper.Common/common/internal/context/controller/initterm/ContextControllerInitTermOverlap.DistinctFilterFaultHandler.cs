using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public partial class ContextControllerInitTermOverlap
    {
        public class DistinctFilterFaultHandler : FilterFaultHandler
        {
            private readonly ContextControllerInitTermWDistinct contextControllerInitTerm;
            private readonly IntSeqKey controllerPath;

            public DistinctFilterFaultHandler(
                ContextControllerInitTermWDistinct contextControllerInitTerm,
                IntSeqKey controllerPath)
            {
                this.contextControllerInitTerm = contextControllerInitTerm;
                this.controllerPath = controllerPath;
            }

            public bool HandleFilterFault(
                EventBean theEvent,
                long version)
            {
                // Handle filter faults such as, for hashed non-preallocated-context, for example:
                // - a) App thread determines event E1 applies to CTX + CP1
                // b) Timer thread destroys CP1
                // c) App thread processes E1 for CTX allocating CP2, processing E1 for CP2
                // d) App thread processes E1 for CP1, filter-faulting and ending up dropping the event for CP1 because of this handler
                // - a) App thread determines event E1 applies to CTX + CP1
                // b) App thread processes E1 for CTX, no action
                // c) Timer thread destroys CP1
                // d) App thread processes E1 for CP1, filter-faulting and ending up processing E1 into CTX because of this handler
                var aiCreate = contextControllerInitTerm.Realization.AgentInstanceContextCreate;
                var @lock = aiCreate.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
                
                using (@lock.AcquireWriteLock())
                {
                    var key = contextControllerInitTerm.GetDistinctKey(theEvent);
                    var trigger = contextControllerInitTerm.DistinctLastTriggerEvents.Get(key);

                    // see if we find that context partition
                    if (trigger != null) {
                        // true for we have already handled this event
                        // false for filter fault
                        return trigger.Equals(theEvent);
                    }

                    // not found: evaluate against context
                    AgentInstanceUtil.EvaluateEventForStatement(
                        theEvent,
                        null,
                        Collections.SingletonList(new AgentInstance(null, aiCreate, null)),
                        aiCreate);

                    return true; // we handled the event
                }
            }
        }
    }
}