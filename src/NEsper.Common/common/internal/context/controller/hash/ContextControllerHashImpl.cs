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
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashImpl : ContextControllerHash
    {
        private readonly ContextControllerHashSvc _hashSvc;

        public ContextControllerHashImpl(
            ContextControllerHashFactory factory,
            ContextManagerRealization realization)
            : base(realization, factory)
        {
            _hashSvc = ContextControllerHashUtil.MakeService(factory, realization);
        }

        public override void Activate(
            IntSeqKey path,
            object[] parentPartitionKeys,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern)
        {
            _hashSvc.MgmtCreate(path, parentPartitionKeys);

            if (factory.HashSpec.IsPreallocate) {
                int[] subpathOrCPIds = ActivateByPreallocate(path, parentPartitionKeys, optionalTriggeringEvent);
                _hashSvc.MgmtSetSubpathOrCPIdsWhenPreallocate(path, subpathOrCPIds);
                return;
            }

            ContextControllerDetailHashItem[] hashItems = factory.HashSpec.Items;
            ContextControllerFilterEntry[] filterEntries = new ContextControllerFilterEntry[hashItems.Length];

            for (int i = 0; i < hashItems.Length; i++) {
                ContextControllerDetailHashItem item = hashItems[i];
                filterEntries[i] = new ContextControllerHashFilterEntry(this, path, item, parentPartitionKeys);

                if (optionalTriggeringEvent != null) {
                    bool match = AgentInstanceUtil.EvaluateFilterForStatement(
                        optionalTriggeringEvent,
                        realization.AgentInstanceContextCreate,
                        filterEntries[i].FilterHandle);

                    if (match) {
                        MatchFound(item, optionalTriggeringEvent, path);
                    }
                }
            }

            _hashSvc.MgmtSetFilters(path, filterEntries);
        }

        public override void Deactivate(
            IntSeqKey path,
            bool terminateChildContexts)
        {
            if (factory.HashSpec.IsPreallocate && terminateChildContexts) {
                int[] subpathOrCPIdsX = _hashSvc.MgmtGetSubpathOrCPIdsWhenPreallocate(path);
                for (int i = 0; i < factory.HashSpec.Granularity; i++) {
                    realization.ContextPartitionTerminate(path, subpathOrCPIdsX[i], this, null, false, null);
                }

                return;
            }

            ContextControllerFilterEntry[] filters = _hashSvc.MgmtGetFilters(path);
            if (filters != null) {
                foreach (ContextControllerFilterEntry callback in filters) {
                    ((ContextControllerHashFilterEntry) callback).Destroy();
                }
            }

            foreach (int id in _hashSvc.Deactivate(path)) {
                realization.ContextPartitionTerminate(path, id, this, null, false, null);
            }
        }

        public void MatchFound(
            ContextControllerDetailHashItem item,
            EventBean theEvent,
            IntSeqKey controllerPath)
        {
            int value = item.Lookupable.Eval.Eval(theEvent, realization.AgentInstanceContextCreate).AsInt32();
            if (_hashSvc.HashHasSeenPartition(controllerPath, value)) {
                return;
            }

            object[] parentPartitionKeys = _hashSvc.MgmtGetParentPartitionKeys(controllerPath);
            ContextPartitionInstantiationResult result = realization.ContextPartitionInstantiate(
                controllerPath,
                value,
                this,
                theEvent,
                null,
                parentPartitionKeys,
                value);
            int subpathIdOrCPId = result.SubpathOrCPId;
            _hashSvc.HashAddPartition(controllerPath, value, subpathIdOrCPId);

            // update the filter version for this handle
            long filterVersion = realization.AgentInstanceContextCreate.FilterService.FiltersVersion;
            realization.AgentInstanceContextCreate.EpStatementAgentInstanceHandle.StatementFilterVersion
                .StmtFilterVersion = filterVersion;
        }

        protected override void VisitPartitions(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId)
        {
            _hashSvc.HashVisit(controllerPath, hashAndCPId);
        }

        protected override int GetSubpathOrCPId(
            IntSeqKey path,
            int hash)
        {
            return _hashSvc.HashGetSubpathOrCPId(path, hash);
        }

        public override void Destroy()
        {
            _hashSvc.Destroy();
        }
        
        
        public override void Transfer(IntSeqKey path, bool transferChildContexts, AgentInstanceTransferServices xfer) {
            if (!factory.HashSpec.IsPreallocate) {
                ContextControllerFilterEntry[] filterEntries = _hashSvc.MgmtGetFilters(path);
                ContextControllerDetailHashItem[] hashItems = factory.HashSpec.Items;
                for (int i = 0; i < hashItems.Length; i++) {
                    filterEntries[i].Transfer(hashItems[i].FilterSpecActivatable, xfer);
                }
            }

            if (!transferChildContexts) {
                return;
            }

            VisitPartitions(path, (hash, subpathOrCPId) => realization.TransferRecursive(path, subpathOrCPId, this, xfer));
        }
    }
} // end of namespace