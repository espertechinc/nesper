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
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedImpl : ContextControllerKeyed
    {
        internal readonly ContextControllerKeyedSvc keyedSvc;

        public ContextControllerKeyedImpl(
            ContextControllerKeyedFactory factory,
            ContextManagerRealization realization)
            : base(realization, factory)
        {
            keyedSvc = ContextControllerKeyedUtil.GetService(factory, realization);
        }

        public override void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            keyedSvc.MgmtCreate(path, parentPartitionKeys);
            var filterEntries = ActivateFilters(optionalTriggeringEvent, path, parentPartitionKeys);
            keyedSvc.MgmtSetFilters(path, filterEntries);
        }

        public override void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts)
        {
            if (path.Length != factory.FactoryEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unrecognized controller path");
            }

            var filters = keyedSvc.MgmtGetFilters(path);
            foreach (var callback in filters) {
                ((ContextControllerKeyedFilterEntry) callback).Destroy();
            }

            if (factory.KeyedSpec.OptionalTermination != null) {
                var terminationConditions = keyedSvc.KeyGetTermConditions(path);
                foreach (var condition in terminationConditions) {
                    condition.Deactivate();
                }
            }

            var subpaths = keyedSvc.Deactivate(path);
            if (terminateChildContexts) {
                foreach (var subpathId in subpaths) {
                    realization.ContextPartitionTerminate(path, subpathId, this, null, false, null);
                }
            }
        }

        public void MatchFound(
            ContextControllerDetailKeyedItem item,
            EventBean theEvent,
            IntSeqKey controllerPath,
            string optionalInitCondAsName)
        {
            if (controllerPath.Length != factory.FactoryEnv.NestingLevel - 1) {
                throw new IllegalStateException("Unrecognized controller path");
            }

            var getterKey = item.Getter.Get(theEvent);
            var exists = keyedSvc.KeyHasSeen(controllerPath, getterKey);
            if (exists || theEvent == lastTerminatingEvent) {
                // if all-matches is more than one, the termination has also fired
                return;
            }

            lastTerminatingEvent = null;

            var partitionKey = getterKey;
            if (factory.keyedSpec.HasAsName) {
                partitionKey = new ContextControllerKeyedPartitionKeyWInit(
                    getterKey, optionalInitCondAsName, optionalInitCondAsName == null ? null : theEvent);
            }

            var parentPartitionKeys = keyedSvc.MgmtGetPartitionKeys(controllerPath);

            // get next subpath id
            var subpathId = keyedSvc.MgmtGetIncSubpath(controllerPath);

            // instantiate
            var result = realization.ContextPartitionInstantiate(
                controllerPath, subpathId, this, theEvent, null, parentPartitionKeys, partitionKey);
            var subpathIdOrCPId = result.SubpathOrCPId;

            // handle termination filter
            ContextControllerConditionNonHA terminationCondition = null;
            if (factory.KeyedSpec.OptionalTermination != null) {
                var conditionPath = controllerPath.AddToEnd(subpathIdOrCPId);
                terminationCondition = ActivateTermination(
                    theEvent, parentPartitionKeys, partitionKey, conditionPath, optionalInitCondAsName);

                foreach (var agentInstance in result.AgentInstances) {
                    agentInstance.AgentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler =
                        ContextControllerWTerminationFilterFaultHandler.INSTANCE;
                }
            }

            keyedSvc.KeyAdd(controllerPath, getterKey, subpathIdOrCPId, terminationCondition);

            // update the filter version for this handle
            var filterVersionAfterStart = realization.AgentInstanceContextCreate.FilterService.FiltersVersion;
            realization.AgentInstanceContextCreate.EpStatementAgentInstanceHandle.StatementFilterVersion
                .StmtFilterVersion = filterVersionAfterStart;
        }

        protected override void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<object, int> keyAndSubpathOrCPId)
        {
            keyedSvc.KeyVisit(controllerPath, keyAndSubpathOrCPId);
        }

        protected override int GetSubpathOrCPId(
            IntSeqKey path,
            object keyForLookup)
        {
            return keyedSvc.KeyGetSubpathOrCPId(path, keyForLookup);
        }

        public override void Destroy()
        {
            keyedSvc.Destroy();
        }

        private ContextControllerConditionNonHA ActivateTermination(
            EventBean triggeringEvent,
            object[] parentPartitionKeys,
            object partitionKey,
            IntSeqKey conditionPath,
            string optionalInitCondAsName)
        {
            ContextControllerConditionCallback callback = new ProxyContextControllerConditionCallback(
                (
                    conditionPathArg,
                    originEndpoint,
                    optionalTriggeringEvent,
                    optionalTriggeringPattern,
                    optionalTriggeringEventPattern,
                    optionalPatternForInclusiveEval) => {
                    var parentPath = conditionPathArg.RemoveFromEnd();
                    var getterKey = factory.GetGetterKey(partitionKey);
                    var removed = keyedSvc.KeyRemove(parentPath, getterKey);
                    if (removed == null) {
                        return;
                    }

                    // remember the terminating event, we don't want it to initiate a new partition
                    lastTerminatingEvent = optionalTriggeringEvent != null
                        ? optionalTriggeringEvent
                        : optionalTriggeringEventPattern;
                    realization.ContextPartitionTerminate(
                        conditionPathArg.RemoveFromEnd(), removed.SubpathOrCPId, this, null, false, null);
                    removed.TerminationCondition.Deactivate();
                });

            var partitionKeys = CollectionUtil.AddValue(parentPartitionKeys, partitionKey);
            var terminationCondition = ContextControllerConditionFactory.GetEndpoint(
                conditionPath, partitionKeys, factory.keyedSpec.OptionalTermination, callback, this, false);

            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider = null;
            if (optionalInitCondAsName != null) {
                endConditionMatchEventProvider = new ProxyContextControllerEndConditionMatchEventProvider {
                    ProcPopulateEndConditionFromTrigger = (
                        map,
                        triggeringEventArg) => {
                        ContextControllerKeyedUtil.PopulatePriorMatch(optionalInitCondAsName, map, triggeringEventArg);
                    }
                };
            }

            terminationCondition.Activate(triggeringEvent, endConditionMatchEventProvider);

            return terminationCondition;
        }

        private ContextControllerFilterEntry[] ActivateFilters(
            EventBean optionalTriggeringEvent,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            ContextConditionDescriptor[] optionalInit = factory.KeyedSpec.OptionalInit;
            if (optionalInit == null || optionalInit.Length == 0) {
                return ActivateFiltersFromPartitionKeys(optionalTriggeringEvent, controllerPath, parentPartitionKeys);
            }

            return ActivateFiltersFromInit(optionalTriggeringEvent, controllerPath, parentPartitionKeys);
        }

        private ContextControllerFilterEntry[] ActivateFiltersFromInit(
            EventBean optionalTriggeringEvent,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            var inits = factory.KeyedSpec.OptionalInit;
            var filterEntries = new ContextControllerFilterEntry[inits.Length];
            for (var i = 0; i < inits.Length; i++) {
                var init = inits[i];
                var found = ContextControllerKeyedUtil.FindInitMatchingKey(factory.KeyedSpec.Items, init);
                filterEntries[i] = ActivateFilterWithInit(
                    init, found, optionalTriggeringEvent, controllerPath, parentPartitionKeys);
            }

            return filterEntries;
        }

        private ContextControllerFilterEntry[] ActivateFiltersFromPartitionKeys(
            EventBean optionalTriggeringEvent,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            var items = factory.KeyedSpec.Items;
            var filterEntries = new ContextControllerFilterEntry[items.Length];
            for (var i = 0; i < items.Length; i++) {
                filterEntries[i] = ActivateFilterNoInit(
                    items[i], optionalTriggeringEvent, controllerPath, parentPartitionKeys);
            }

            return filterEntries;
        }

        private ContextControllerFilterEntry ActivateFilterNoInit(
            ContextControllerDetailKeyedItem item,
            EventBean optionalTriggeringEvent,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            var callback = new ContextControllerKeyedFilterEntryNoInit(this, controllerPath, parentPartitionKeys, item);
            if (optionalTriggeringEvent != null) {
                var match = AgentInstanceUtil.EvaluateFilterForStatement(
                    optionalTriggeringEvent, realization.AgentInstanceContextCreate, callback.FilterHandle);

                if (match) {
                    callback.MatchFound(optionalTriggeringEvent, null);
                }
            }

            return callback;
        }

        private ContextControllerFilterEntry ActivateFilterWithInit(
            ContextConditionDescriptorFilter filter,
            ContextControllerDetailKeyedItem item,
            EventBean optionalTriggeringEvent,
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            return new ContextControllerKeyedFilterEntryWInit(this, controllerPath, item, parentPartitionKeys, filter);
        }

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
} // end of namespace