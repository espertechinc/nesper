///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public partial class ContextControllerInitTermOverlap : ContextControllerInitTermBase,
        ContextControllerInitTermWDistinct
    {
        private readonly EventBean[] eventsPerStreamDistinct;

        public ContextControllerInitTermOverlap(
            ContextControllerInitTermFactory factory,
            ContextManagerRealization realization) : base(factory, realization)
        {
            if (factory.InitTermSpec.DistinctEval != null) {
                if (factory.FactoryEnv.NumNestingLevels == 1) {
                    DistinctSvc = new ContextControllerInitTermDistinctSvcNonNested();
                }
                else {
                    DistinctSvc = new ContextControllerInitTermDistinctSvcNested();
                }

                eventsPerStreamDistinct = new EventBean[1];
                DistinctLastTriggerEvents = new LRUCache<object, EventBean>(16);
            }
            else {
                DistinctSvc = null;
                DistinctLastTriggerEvents = null;
                eventsPerStreamDistinct = null;
            }
        }

        public ContextControllerInitTermDistinctSvc DistinctSvc { get; }

        public LRUCache<object, EventBean> DistinctLastTriggerEvents { get; }

        public object GetDistinctKey(EventBean eventBean)
        {
            eventsPerStreamDistinct[0] = eventBean;
            return _factory.InitTermSpec.DistinctEval.Evaluate(
                eventsPerStreamDistinct,
                true,
                realization.AgentInstanceContextCreate);
        }

        public override void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            initTermSvc.MgmtCreate(path, parentPartitionKeys);

            var startCondition = ContextControllerConditionFactory.GetEndpoint(
                path,
                parentPartitionKeys,
                _factory.initTermSpec.StartCondition,
                this,
                this);
            var isTriggeringEventMatchesFilter = startCondition.Activate(
                optionalTriggeringEvent,
                null,
                optionalTriggeringPattern);
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
            if (DistinctSvc != null) {
                DistinctSvc.Clear(path);
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (DistinctSvc != null) {
                DistinctSvc.Destroy();
            }
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
            var endConditionNotification = originCondition.Descriptor != _factory.InitTermSpec.StartCondition;
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

        private void RangeNotificationStart(
            IntSeqKey controllerPath,
            ContextControllerConditionNonHA startCondition,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern)
        {
            if (DistinctSvc != null) {
                var added = AddDistinctKey(controllerPath, optionalTriggeringEvent);
                if (!added) {
                    return;
                }
            }

            // For overlapping mode, make sure we activate again or stay activated
            if (!startCondition.IsRunning) {
                startCondition.Activate(optionalTriggeringEvent, null, optionalTriggeringPattern);
            }

            var agentInstances = InstantiateAndActivateEndCondition(
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

            var instance = initTermSvc.EndDelete(conditionPath);
            if (instance == null) {
                return;
            }

            if (DistinctSvc != null) {
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

            var key = GetDistinctKey(optionalTriggeringEvent);
            DistinctLastTriggerEvents.Put(key, optionalTriggeringEvent);
            return DistinctSvc.AddUnlessExists(controllerPath, key);
        }

        private void RemoveDistinctKey(
            IntSeqKey controllerPath,
            ContextControllerInitTermSvcEntry value)
        {
            var @event = value.PartitionKey.TriggeringEvent;
            var key = GetDistinctKey(@event);
            DistinctSvc.Remove(controllerPath, key);
        }

        private void InstallFilterFaultHandler(
            IList<AgentInstance> agentInstances,
            IntSeqKey controllerPath)
        {
            if (agentInstances.IsEmpty()) {
                return;
            }

            if (DistinctSvc == null) {
                return;
            }

            FilterFaultHandler myFaultHandler = new DistinctFilterFaultHandler(this, controllerPath);
            foreach (var agentInstance in agentInstances) {
                agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = myFaultHandler;
            }
        }
    }
} // end of namespace