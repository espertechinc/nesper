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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermOverlap : ContextControllerInitTermBase,
        ContextControllerInitTermWDistinct
    {
        private readonly ContextControllerInitTermDistinctSvc _distinctSvc;
        private readonly LRUCache<object, EventBean> _distinctLastTriggerEvents;
        private readonly EventBean[] _eventsPerStreamDistinct;

        public ContextControllerInitTermOverlap(
            ContextControllerInitTermFactory factory,
            ContextManagerRealization realization)
            : base(factory, realization)

        {
            if (factory.InitTermSpec.DistinctEval != null) {
                if (factory.FactoryEnv.NumNestingLevels == 1) {
                    _distinctSvc = new ContextControllerInitTermDistinctSvcNonNested();
                }
                else {
                    _distinctSvc = new ContextControllerInitTermDistinctSvcNested();
                }

                _eventsPerStreamDistinct = new EventBean[1];
                _distinctLastTriggerEvents = new LRUCache<object, EventBean>(16);
            }
            else {
                _distinctSvc = null;
                _distinctLastTriggerEvents = null;
                _eventsPerStreamDistinct = null;
            }
        }

        public LRUCache<object, EventBean> DistinctLastTriggerEvents {
            get { return _distinctLastTriggerEvents; }
        }

        public override void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            initTermSvc.MgmtCreate(path, parentPartitionKeys);

            ContextControllerConditionNonHA startCondition = ContextControllerConditionFactory.GetEndpoint(
                path,
                parentPartitionKeys,
                factory.initTermSpec.StartCondition,
                this,
                this,
                true);
            bool isTriggeringEventMatchesFilter = startCondition.Activate(optionalTriggeringEvent, null, optionalTriggeringPattern);
            initTermSvc.MgmtUpdSetStartCondition(path, startCondition);

            if (isTriggeringEventMatchesFilter || startCondition.IsImmediate) {
                InstantiateAndActivateEndCondition(
                    path,
                    optionalTriggeringEvent,
                    optionalTriggeringPattern,
                    optionalTriggeringPattern,
                    startCondition);
            }
        }

        public override void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts)
        {
            base.Deactivate(path, terminateChildContexts);
            _distinctSvc?.Clear(path);
        }

        public override void Destroy()
        {
            base.Destroy();
            _distinctSvc?.Destroy();
        }

        public override void RangeNotification(
            IntSeqKey conditionPath,
            ContextControllerConditionNonHA originCondition,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            IDictionary<string, object> terminationProperties)
        {
            bool endConditionNotification = originCondition.Descriptor != factory.InitTermSpec.StartCondition;
            if (endConditionNotification) {
                RangeNotificationEnd(
                    conditionPath,
                    originCondition,
                    optionalTriggeringEvent,
                    optionalTriggeringPattern,
                    optionalTriggeringEventPattern,
                    terminationProperties);
            }
            else {
                RangeNotificationStart(
                    conditionPath,
                    originCondition,
                    optionalTriggeringEvent,
                    optionalTriggeringPattern,
                    optionalTriggeringEventPattern);
            }
        }

        public ContextControllerInitTermDistinctSvc DistinctSvc {
            get => _distinctSvc;
        }

        private void RangeNotificationStart(
            IntSeqKey controllerPath,
            ContextControllerConditionNonHA startCondition,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern)
        {
            if (_distinctSvc != null) {
                bool added = AddDistinctKey(controllerPath, optionalTriggeringEvent);
                if (!added) {
                    return;
                }
            }

            // For overlapping mode, make sure we activate again or stay activated
            if (!startCondition.IsRunning) {
                startCondition.Activate(optionalTriggeringEvent, null, optionalTriggeringPattern);
            }

            IList<AgentInstance> agentInstances = InstantiateAndActivateEndCondition(
                controllerPath,
                optionalTriggeringEvent,
                optionalTriggeringPattern,
                optionalTriggeringPattern,
                startCondition);
            InstallFilterFaultHandler(agentInstances, controllerPath);
        }

        private void RangeNotificationEnd(
            IntSeqKey conditionPath,
            ContextControllerConditionNonHA endCondition,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> terminationProperties)
        {
            if (endCondition.IsRunning) {
                endCondition.Deactivate();
            }

            ContextControllerInitTermSvcEntry instance = initTermSvc.EndDelete(conditionPath);
            if (instance == null) {
                return;
            }

            if (_distinctSvc != null) {
                RemoveDistinctKey(conditionPath.RemoveFromEnd(), instance);
            }

            realization.ContextPartitionTerminate(
                conditionPath.RemoveFromEnd(),
                instance.SubpathIdOrCPId,
                this,
                terminationProperties,
                false,
                null);
        }

        private bool AddDistinctKey(
            IntSeqKey controllerPath,
            EventBean optionalTriggeringEvent)
        {
            if (optionalTriggeringEvent == null) {
                throw new IllegalStateException("No trgiggering event provided");
            }

            object key = GetDistinctKey(optionalTriggeringEvent);
            _distinctLastTriggerEvents.Put(key, optionalTriggeringEvent);
            return _distinctSvc.AddUnlessExists(controllerPath, key);
        }

        private void RemoveDistinctKey(
            IntSeqKey controllerPath,
            ContextControllerInitTermSvcEntry value)
        {
            EventBean @event = value.PartitionKey.TriggeringEvent;
            object key = GetDistinctKey(@event);
            _distinctSvc.Remove(controllerPath, key);
        }

        public object GetDistinctKey(EventBean eventBean)
        {
            _eventsPerStreamDistinct[0] = eventBean;
            return factory.InitTermSpec.DistinctEval.Evaluate(
                _eventsPerStreamDistinct,
                true,
                realization.AgentInstanceContextCreate);
        }

        private void InstallFilterFaultHandler(
            IList<AgentInstance> agentInstances,
            IntSeqKey controllerPath)
        {
            if (agentInstances.IsEmpty()) {
                return;
            }

            if (_distinctSvc == null) {
                return;
            }

            FilterFaultHandler myFaultHandler = new DistinctFilterFaultHandler(this, controllerPath);
            foreach (AgentInstance agentInstance in agentInstances) {
                agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = myFaultHandler;
            }
        }

        public class DistinctFilterFaultHandler : FilterFaultHandler
        {
            private readonly ContextControllerInitTermWDistinct _contextControllerInitTerm;
            private readonly IntSeqKey _controllerPath;

            public DistinctFilterFaultHandler(
                ContextControllerInitTermWDistinct contextControllerInitTerm,
                IntSeqKey controllerPath)
            {
                this._contextControllerInitTerm = contextControllerInitTerm;
                this._controllerPath = controllerPath;
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
                var aiCreate = _contextControllerInitTerm.Realization.AgentInstanceContextCreate;
                using (aiCreate.EpStatementAgentInstanceHandle.StatementAgentInstanceLock.AcquireWriteLock())
                {
                    object key = _contextControllerInitTerm.GetDistinctKey(theEvent);
                    EventBean trigger = _contextControllerInitTerm.DistinctLastTriggerEvents.Get(key);

                    // see if we find that context partition
                    if (trigger != null)
                    {
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
} // end of namespace