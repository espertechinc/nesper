///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.context.controller.initterm.ContextControllerInitTermUtil;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermNonOverlap : ContextControllerInitTermBase,
        ContextControllerInitTermWLastTrigger
    {
        public ContextControllerInitTermNonOverlap(
            ContextControllerInitTermFactory factory, ContextManagerRealization realization) : base(
            factory, realization)
        {
        }

        public EventBean LastTriggerEvent { get; private set; }

        public override void Activate(
            IntSeqKey path, object[] parentPartitionKeys, EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            initTermSvc.MgmtCreate(path, parentPartitionKeys);

            var startCondition = ContextControllerConditionFactory.GetEndpoint(
                path, parentPartitionKeys, factory.initTermSpec.StartCondition, this, this, true);
            var currentlyRunning = DetermineCurrentlyRunning(startCondition, this);

            if (!currentlyRunning) {
                initTermSvc.MgmtUpdSetStartCondition(path, startCondition);
                var isTriggeringEventMatchesFilter = startCondition.Activate(optionalTriggeringEvent, null);
                if (isTriggeringEventMatchesFilter) {
                    RangeNotificationStart(path, optionalTriggeringEvent, null, null, null);
                }
            }
            else {
                InstantiateAndActivateEndCondition(
                    path, optionalTriggeringEvent, optionalTriggeringPattern, optionalTriggeringPattern,
                    startCondition);
            }
        }

        public override void RangeNotification(
            IntSeqKey conditionPath, 
            ContextControllerConditionNonHA originCondition, EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern, EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval)
        {
            bool endConditionNotification = originCondition.Descriptor != factory.InitTermSpec.StartCondition;
            if (endConditionNotification) {
                RangeNotificationEnd(
                    conditionPath, originCondition, optionalTriggeringEvent, optionalTriggeringPattern,
                    optionalTriggeringEventPattern);
            }
            else {
                LastTriggerEvent = optionalTriggeringEvent;
                RangeNotificationStart(
                    conditionPath, optionalTriggeringEvent, optionalTriggeringPattern, optionalTriggeringEventPattern,
                    optionalPatternForInclusiveEval);
            }
        }

        private void RangeNotificationStart(
            IntSeqKey controllerPath, EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern, EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval)
        {
            var startCondition = initTermSvc.MgmtUpdClearStartCondition(controllerPath);
            if (startCondition.IsRunning) {
                startCondition.Deactivate();
            }

            var agentInstances = InstantiateAndActivateEndCondition(
                controllerPath, optionalTriggeringEvent, optionalTriggeringPattern, optionalPatternForInclusiveEval,
                startCondition);
            InstallFilterFaultHandler(agentInstances, controllerPath);
        }

        private void RangeNotificationEnd(
            IntSeqKey conditionPath, ContextControllerConditionNonHA endCondition, EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern, EventBean optionalTriggeringEventPattern)
        {
            if (endCondition.IsRunning) {
                endCondition.Deactivate();
            }

            var instance = initTermSvc.EndDelete(conditionPath);
            if (instance == null) {
                return;
            }

            // start "@now" we maintain the locks
            var startNow = factory.InitTermSpec.StartCondition is ContextConditionDescriptorImmediate;
            IList<AgentInstance> agentInstancesLocksHeld = null;
            if (startNow) {
                realization.AgentInstanceContextCreate.FilterService.AcquireWriteLock();
                agentInstancesLocksHeld = new List<AgentInstance>(2);
            }

            realization.ContextPartitionTerminate(
                conditionPath.RemoveFromEnd(), instance.SubpathIdOrCPId, this, optionalTriggeringPattern, startNow,
                agentInstancesLocksHeld);

            try {
                var controllerPath = conditionPath.RemoveFromEnd();
                var partitionKeys = initTermSvc.MgmtGetParentPartitionKeys(controllerPath);

                var startDesc = factory.initTermSpec.StartCondition;
                var startCondition = ContextControllerConditionFactory.GetEndpoint(
                    controllerPath, partitionKeys, startDesc, this, this, true);
                if (!startCondition.IsImmediate) {
                    startCondition.Activate(optionalTriggeringEvent, null);
                    initTermSvc.MgmtUpdSetStartCondition(controllerPath, startCondition);
                }
                else {
                    // we do not forward triggering events of termination
                    InstantiateAndActivateEndCondition(controllerPath, null, null, null, startCondition);
                }
            }
            finally {
                if (agentInstancesLocksHeld != null) {
                    foreach (var agentInstance in agentInstancesLocksHeld) {
                        agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion
                            .StmtFilterVersion = long.MaxValue;
                        if (agentInstance.AgentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                            agentInstance.AgentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                        }

                        agentInstance.AgentInstanceContext.AgentInstanceLock.ReleaseWriteLock();
                    }
                }

                if (startNow) {
                    realization.AgentInstanceContextCreate.FilterService.ReleaseWriteLock();
                }
            }
        }

        private void InstallFilterFaultHandler(IList<AgentInstance> agentInstances, IntSeqKey controllerPath)
        {
            if (agentInstances.IsEmpty()) {
                return;
            }

            if (!(factory.InitTermSpec.StartCondition is ContextConditionDescriptorFilter)) {
                return;
            }

            FilterFaultHandler myFaultHandler = new NonOverlapWFIlterStartFilterFaultHandler(this);
            foreach (var agentInstance in agentInstances) {
                agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = myFaultHandler;
            }
        }

        public class NonOverlapWFIlterStartFilterFaultHandler : FilterFaultHandler
        {
            private readonly ContextControllerInitTermWLastTrigger contextControllerInitTerm;

            public NonOverlapWFIlterStartFilterFaultHandler(
                ContextControllerInitTermWLastTrigger contextControllerInitTerm)
            {
                this.contextControllerInitTerm = contextControllerInitTerm;
            }

            public bool HandleFilterFault(EventBean theEvent, long version)
            {
                // Handle filter faults such as
                // - a) App thread determines event E1 applies to CP1
                // b) Timer thread destroys CP1
                // c) App thread processes E1 for CP1, filter-faulting and ending up reprocessing the event against CTX because of this handler
                var aiCreate = contextControllerInitTerm.Realization.AgentInstanceContextCreate;
                var @lock = aiCreate.EpStatementAgentInstanceHandle.StatementAgentInstanceLock;
                @lock.AcquireWriteLock();
                try {
                    var trigger = contextControllerInitTerm.LastTriggerEvent;
                    if (theEvent != trigger) {
                        AgentInstanceUtil.EvaluateEventForStatement(
                            theEvent, null, Collections.SingletonList(new AgentInstance(null, aiCreate, null)),
                            aiCreate);
                    }

                    return true; // we handled the event
                }
                finally {
                    @lock.ReleaseWriteLock();
                }
            }
        }
    }
} // end of namespace