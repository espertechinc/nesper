///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;


namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public abstract class ContextControllerInitTerm : ContextController
    {
        protected readonly ContextControllerInitTermFactory _factory;
        protected readonly ContextManagerRealization realization;

        public ContextControllerInitTerm(
            ContextControllerInitTermFactory factory,
            ContextManagerRealization realization)
        {
            _factory = factory;
            this.realization = realization;
        }

        protected abstract void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId);

        public void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector selector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (selector is ContextPartitionSelectorFiltered filter) {
                VisitPartitions(
                    path,
                    (
                        partitionKey,
                        subpathOrCPIds) => {
                        var identifier = ContextControllerInitTermUtil.KeyToIdentifier(
                            subpathOrCPIds,
                            partitionKey,
                            this);
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

            if (selector is ContextPartitionSelectorById byId) {
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

            throw ContextControllerSelectorUtil.GetInvalidSelector(Type.EmptyTypes, selector);
        }

        public void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            EventBean triggeringEvent)
        {
            // compute correlated termination
            var start = _factory.InitTermSpec.StartCondition;
            if (!(start is ContextConditionDescriptorFilter filter)) {
                return;
            }

            if (filter.OptionalFilterAsName == null) {
                return;
            }

            var tag = map.Meta.GetTagFor(filter.OptionalFilterAsName);
            if (tag == -1) {
                return;
            }

            map.Add(tag, triggeringEvent);
        }

        public void PopulateEndConditionFromTrigger(
            MatchedEventMap map,
            IDictionary<string, object> matchedEventMap,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            // compute correlated termination
            var start = _factory.InitTermSpec.StartCondition;
            if (!(start is ContextConditionDescriptorPattern pattern)) {
                return;
            }

            if (pattern.AsName == null) {
                foreach (var tagged in pattern.TaggedEvents) {
                    PopulatePattern(tagged, map, matchedEventMap);
                }

                foreach (var array in pattern.ArrayEvents) {
                    PopulatePattern(array, map, matchedEventMap);
                }
            }
            else {
                foreach (var entry in matchedEventMap) {
                    var tag = map.Meta.GetTagFor(entry.Key);
                    if (tag == -1) {
                        return;
                    }

                    map.Add(tag, entry.Value);
                }
            }
        }

        private void PopulatePattern(
            string tagged,
            MatchedEventMap map,
            IDictionary<string, object> matchedEventMap)
        {
            var value = matchedEventMap.Get(tagged);
            if (value == null) {
                return;
            }

            var tag = map.Meta.GetTagFor(tagged);
            if (tag == -1) {
                return;
            }

            map.Add(tag, value);
        }

        public ContextControllerFactory Factory => _factory;

        public ContextControllerInitTermFactory InitTermFactory => _factory;
        
        public ContextManagerRealization Realization => realization;

        public abstract void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern);

        public abstract void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts);

        public abstract void Transfer(
            IntSeqKey path,
            bool transferChildContexts,
            AgentInstanceTransferServices xfer);

        public abstract void Destroy();
    }
} // end of namespace