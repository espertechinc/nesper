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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public abstract class ContextControllerInitTermBase : ContextControllerInitTerm,
        ContextControllerConditionCallback,
        ContextControllerEndConditionMatchEventProvider
    {
        internal readonly ContextControllerInitTermSvc initTermSvc;

        public ContextControllerInitTermBase(
            ContextControllerInitTermFactory factory,
            ContextManagerRealization realization)
            : base(factory, realization)
        {
            initTermSvc = ContextControllerInitTermUtil.GetService(factory);
        }

        public abstract void RangeNotification(
            IntSeqKey conditionPath,
            ContextControllerConditionNonHA originEndpoint,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            EventBean optionalTriggeringEventPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            IDictionary<string, object> terminationProperties);

        public override void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts)
        {
            var initCondition = initTermSvc.MgmtDelete(path);
            if (initCondition != null) {
                if (initCondition.IsRunning) {
                    initCondition.Deactivate();
                }
            }

            var endConditions = initTermSvc.EndDeleteByParentPath(path);
            foreach (var entry in endConditions) {
                if (entry.TerminationCondition.IsRunning) {
                    entry.TerminationCondition.Deactivate();
                }

                if (terminateChildContexts) {
                    realization.ContextPartitionTerminate(path, entry.SubpathIdOrCPId, this, null, false, null);
                }
            }
        }

        protected override void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId)
        {
            initTermSvc.EndVisit(controllerPath, partKeyAndCPId);
        }

        public override void Destroy()
        {
            initTermSvc.Destroy();
        }

        internal IList<AgentInstance> InstantiateAndActivateEndCondition(
            IntSeqKey controllerPath,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            IDictionary<string, object> optionalPatternForInclusiveEval,
            ContextControllerConditionNonHA startCondition)
        {
            var subpathId = initTermSvc.MgmtUpdIncSubpath(controllerPath);

            var endConditionPath = controllerPath.AddToEnd(subpathId);
            var partitionKeys = initTermSvc.MgmtGetParentPartitionKeys(controllerPath);
            var endCondition = ContextControllerConditionFactory.GetEndpoint(
                endConditionPath,
                partitionKeys,
                _factory.initTermSpec.EndCondition,
                this,
                this);
            endCondition.Activate(optionalTriggeringEvent, this, optionalTriggeringPattern);

            var partitionKey = ContextControllerInitTermUtil.BuildPartitionKey(
                optionalTriggeringEvent,
                optionalTriggeringPattern,
                endCondition,
                this);

            var result = realization.ContextPartitionInstantiate(
                controllerPath,
                subpathId,
                this,
                optionalTriggeringEvent,
                optionalPatternForInclusiveEval,
                partitionKeys,
                partitionKey);
            var subpathIdOrCPId = result.SubpathOrCPId;

            initTermSvc.EndCreate(endConditionPath, subpathIdOrCPId, endCondition, partitionKey);

            return result.AgentInstances;
        }

        public override void Transfer(
            IntSeqKey path,
            bool transferChildContexts,
            AgentInstanceTransferServices xfer)
        {
            var start = initTermSvc.MgmtGetStartCondition(path);
            start?.Transfer(xfer);

            initTermSvc.EndVisitConditions(
                path,
                (
                    condition,
                    subPathId) => {
                    condition?.Transfer(xfer);
                });

            if (transferChildContexts) {
                VisitPartitions(
                    path,
                    (
                        partitionKey,
                        subpathOrCPIds) => {
                        realization.TransferRecursive(path, subpathOrCPIds, this, xfer);
                    });
            }
        }
    }
} // end of namespace