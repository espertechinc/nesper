///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public abstract class ContextControllerInitTerm : ContextController
    {
        internal readonly ContextControllerInitTermFactory factory;
        internal readonly ContextManagerRealization realization;

        public ContextControllerInitTerm(
            ContextControllerInitTermFactory factory,
            ContextManagerRealization realization)
        {
            this.factory = factory;
            this.realization = realization;
        }

        protected abstract void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId);

        public abstract void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern);

        public abstract void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts);

        public abstract void Destroy();

        public virtual void Transfer(
            IntSeqKey path,
            bool transferChildContexts,
            AgentInstanceTransferServices xfer)
        {
        }

        public ContextControllerInitTermFactory InitTermFactory {
            get => factory;
        }

        public ContextControllerFactory Factory {
            get => factory;
        }

        public ContextManagerRealization Realization {
            get => realization;
        }

        public void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector selector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (selector is ContextPartitionSelectorFiltered) {
                ContextPartitionSelectorFiltered filter = (ContextPartitionSelectorFiltered) selector;
                VisitPartitions(
                    path,
                    (
                        partitionKey,
                        subpathOrCPIds) => {
                        ContextPartitionIdentifierInitiatedTerminated identifier =
                            ContextControllerInitTermUtil.KeyToIdentifier(subpathOrCPIds, partitionKey, this);
                        if (filter.Filter(identifier)) {
                            realization.ContextPartitionRecursiveVisit(
                                path,
                                subpathOrCPIds,
                                this,
                                visitor,
                                selectorPerLevel);
                        }
                    });
                return;
            }

            if (selector is ContextPartitionSelectorAll) {
                VisitPartitions(
                    path,
                    (
                        partitionKey,
                        subpathOrCPIds) => {
                        realization.ContextPartitionRecursiveVisit(
                            path,
                            subpathOrCPIds,
                            this,
                            visitor,
                            selectorPerLevel);
                    });
                return;
            }

            if (selector is ContextPartitionSelectorById) {
                ContextPartitionSelectorById byId = (ContextPartitionSelectorById) selector;
                VisitPartitions(
                    path,
                    (
                        hash,
                        subpathOrCPId) => {
                        if (byId.ContextPartitionIds.Contains(subpathOrCPId)) {
                            realization.ContextPartitionRecursiveVisit(
                                path,
                                subpathOrCPId,
                                this,
                                visitor,
                                selectorPerLevel);
                        }
                    });
                return;
            }

            throw ContextControllerSelectorUtil.GetInvalidSelector(new Type[0], selector);
        }

        public void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            EventBean triggeringEvent)
        {
            // compute correlated termination
            ContextConditionDescriptor start = factory.InitTermSpec.StartCondition;

            ContextConditionDescriptorFilter filter = start as ContextConditionDescriptorFilter;
            if (filter?.OptionalFilterAsName == null) {
                return;
            }

            int tag = map.Meta.GetTagFor(filter.OptionalFilterAsName);
            if (tag == -1) {
                return;
            }

            map.Add(tag, triggeringEvent);
        }
        
        public void PopulateEndConditionFromTrigger(MatchedEventMap map, IDictionary<String, Object> matchedEventMap) {
            // compute correlated termination
            ContextConditionDescriptor start = factory.InitTermSpec.StartCondition;
            if (!(start is ContextConditionDescriptorPattern)) {
                return;
            }
            ContextConditionDescriptorPattern pattern = (ContextConditionDescriptorPattern) start;
            foreach (String tagged in pattern.TaggedEvents) {
                PopulatePattern(tagged, map, matchedEventMap);
            }
            foreach (String array in pattern.ArrayEvents) {
                PopulatePattern(array, map, matchedEventMap);
            }
        }

        private void PopulatePattern(String tagged, MatchedEventMap map, IDictionary<String, Object> matchedEventMap) {
            if (matchedEventMap.TryGetValue(tagged, out var value)) {
                int tag = map.Meta.GetTagFor(tagged);
                if (tag != -1) {
                    map.Add(tag, value);
                }
            }
        }
    }
} // end of namespace