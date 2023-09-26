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

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public abstract class ContextControllerHash : ContextControllerBase
    {
        protected internal readonly ContextControllerHashFactory factory;

        public ContextControllerHash(
            ContextManagerRealization realization,
            ContextControllerHashFactory factory)
            : base(realization)
        {
            this.factory = factory;
        }

        public AgentInstanceContext AgentInstanceContextCreate => realization.AgentInstanceContextCreate;

        public override ContextControllerFactory Factory => factory;

        public ContextControllerHashFactory HashFactory => factory;

        public override ContextManagerRealization Realization => realization;

        protected abstract void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId);

        protected abstract int GetSubpathOrCPId(
            IntSeqKey path,
            int hash);

        public override void VisitSelectedPartitions(
            IntSeqKey path,
            ContextPartitionSelector selector,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel)
        {
            if (selector is ContextPartitionSelectorHash selectorHash) {
                if (selectorHash.Hashes == null || selectorHash.Hashes.IsEmpty()) {
                    return;
                }

                foreach (var hash in selectorHash.Hashes) {
                    var subpathOrCPId = GetSubpathOrCPId(path, hash);
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

            if (selector is ContextPartitionSelectorFiltered filter) {
                var identifierHash = new ContextPartitionIdentifierHash();

                VisitPartitions(
                    path,
                    (
                        hash,
                        subpathOrCPId) => {
                        identifierHash.Hash = hash;
                        if (factory.FactoryEnv.IsLeaf) {
                            identifierHash.ContextPartitionId = subpathOrCPId;
                        }

                        if (filter.Filter(identifierHash)) {
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
                        hash,
                        subpathOrCPId) => realization.ContextPartitionRecursiveVisit(
                        path,
                        subpathOrCPId,
                        this,
                        visitor,
                        selectorPerLevel));
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

            throw ContextControllerSelectorUtil.GetInvalidSelector(
                new[] { typeof(ContextPartitionSelectorHash) },
                selector);
        }

        protected int[] ActivateByPreallocate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent)
        {
            var granularity = factory.HashSpec.Granularity;
            var cpOrSubpathIds = new int[granularity];
            for (var i = 0; i < factory.HashSpec.Granularity; i++) {
                var result = realization.ContextPartitionInstantiate(
                    path,
                    i,
                    this,
                    optionalTriggeringEvent,
                    null,
                    parentPartitionKeys,
                    i);
                cpOrSubpathIds[i] = result.SubpathOrCPId;
            }

            return cpOrSubpathIds;
        }
    }
} // end of namespace