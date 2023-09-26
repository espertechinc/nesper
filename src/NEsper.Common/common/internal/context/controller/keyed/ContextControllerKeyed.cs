///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public abstract class ContextControllerKeyed : ContextControllerBase
    {
        private readonly ContextControllerKeyedFactory factory;
        private EventBean lastTerminatingEvent;

        public ContextControllerKeyed(
            ContextManagerRealization realization,
            ContextControllerKeyedFactory factory)
            : base(realization)
        {
            this.factory = factory;
        }

        public AgentInstanceContext AgentInstanceContextCreate => realization.AgentInstanceContextCreate;

        public ContextControllerKeyedFactory KeyedFactory => factory;

        public override ContextControllerFactory Factory => factory;

        public override ContextManagerRealization Realization => realization;

        public EventBean LastTerminatingEvent {
            get => lastTerminatingEvent;
            set => lastTerminatingEvent = value;
        }

        protected abstract void VisitPartitions(
            IntSeqKey path,
            BiConsumer<object, int> keyAndSubpathOrCPId);

        protected abstract int GetSubpathOrCPId(
            IntSeqKey path,
            object keyForLookup);

        public override void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector selector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (selector is ContextPartitionSelectorSegmented partitioned) {
                if (partitioned.PartitionKeys == null || partitioned.PartitionKeys.IsEmpty()) {
                    return;
                }

                foreach (var key in partitioned.PartitionKeys) {
                    var keyForLookup = factory.KeyedSpec.MultiKeyFromObjectArray.From(key);
                    var subpathOrCPId = GetSubpathOrCPId(path, keyForLookup);
                    if (subpathOrCPId != -1) {
                        realization.ContextPartitionRecursiveVisit(
                            path,
                            subpathOrCPId,
                            this,
                            visitor,
                            selectorPerLevel);
                    }
                }

                return;
            }

            if (selector is ContextPartitionSelectorFiltered filtered) {
                var identifier = new ContextPartitionIdentifierPartitioned();
                VisitPartitions(
                    path,
                    (
                        key,
                        subpathOrCPId) => {
                        if (factory.FactoryEnv.IsLeaf) {
                            identifier.ContextPartitionId = subpathOrCPId;
                        }

                        var keys = ContextControllerKeyedUtil.UnpackKey(key);
                        identifier.Keys = keys;
                        if (filtered.Filter(identifier)) {
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

            if (selector is ContextPartitionSelectorAll) {
                VisitPartitions(
                    path,
                    (
                        key,
                        subpathOrCPId) => realization.ContextPartitionRecursiveVisit(
                        path,
                        subpathOrCPId,
                        this,
                        visitor,
                        selectorPerLevel));
                return;
            }

            if (selector is ContextPartitionSelectorById ids) {
                VisitPartitions(
                    path,
                    (
                        key,
                        subpathOrCPId) => {
                        if (ids.ContextPartitionIds.Contains(subpathOrCPId)) {
                            realization.ContextPartitionRecursiveVisit(
                                path,
                                subpathOrCPId,
                                this,
                                visitor,
                                selectorPerLevel);
                        }
                    });
            }

            throw ContextControllerSelectorUtil.GetInvalidSelector(
                new[] { typeof(ContextPartitionSelectorSegmented) },
                selector);
        }
    }
} // end of namespace