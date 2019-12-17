///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermSvcLevelAny : ContextControllerInitTermSvc
    {
        private IDictionary<IntSeqKey, ContextControllerInitTermSvcEntry> endConditions =
            new Dictionary<IntSeqKey, ContextControllerInitTermSvcEntry>();

        private IDictionary<IntSeqKey, NestedEntry> mgmt = new Dictionary<IntSeqKey, NestedEntry>();

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            var existing = mgmt.PutIfAbsent(controllerPath, new NestedEntry(0, null, parentPartitionKeys));
            if (existing != null) {
                throw new IllegalStateException("Unexpected existing entry for path");
            }
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            var entry = mgmt.Get(controllerPath);
            return entry == null ? null : entry.parentPartitionKeys;
        }

        public ContextControllerConditionNonHA MgmtDelete(IntSeqKey controllerPath)
        {
            var existing = mgmt.Delete(controllerPath);
            return existing == null ? null : existing.startCondition;
        }

        public ContextControllerConditionNonHA MgmtUpdClearStartCondition(IntSeqKey controllerPath)
        {
            var existing = mgmt.Get(controllerPath);
            ContextControllerConditionNonHA tmp = null;
            if (existing != null) {
                tmp = existing.startCondition;
                existing.startCondition = null;
            }

            return tmp;
        }

        public void MgmtUpdSetStartCondition(
            IntSeqKey controllerPath,
            ContextControllerConditionNonHA startCondition)
        {
            var existing = mgmt.Get(controllerPath);
            if (existing != null) {
                existing.startCondition = startCondition;
            }
        }

        public int MgmtUpdIncSubpath(IntSeqKey controllerPath)
        {
            var existing = mgmt.Get(controllerPath);
            if (existing == null) {
                throw new IllegalStateException("Unexpected no-entry-found for path");
            }

            return existing.currentSubpath++;
        }

        public void EndCreate(
            IntSeqKey endConditionPath,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA endCondition,
            ContextControllerInitTermPartitionKey partitionKey)
        {
            endConditions.Put(
                endConditionPath,
                new ContextControllerInitTermSvcEntry(subpathIdOrCPId, endCondition, partitionKey));
        }

        public ContextControllerInitTermSvcEntry EndDelete(IntSeqKey conditionPath)
        {
            return endConditions.Delete(conditionPath);
        }

        public ICollection<ContextControllerInitTermSvcEntry> EndDeleteByParentPath(IntSeqKey controllerPath)
        {
            var entries = new List<ContextControllerInitTermSvcEntry>();

            endConditions
                .Where(entry => controllerPath.IsParentTo(entry.Key))
                .ToList()
                .ForEach(
                    entry => {
                        entries.Add(entry.Value);
                        endConditions.Remove(entry.Key);
                    });

            return entries;
        }

        public void EndVisit(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId)
        {
            foreach (var entry in endConditions) {
                if (controllerPath.IsParentTo(entry.Key)) {
                    partKeyAndCPId.Invoke(entry.Value.PartitionKey, entry.Value.SubpathIdOrCPId);
                }
            }
        }

        public void Destroy()
        {
            mgmt = null;
            endConditions = null;
        }

        internal class NestedEntry
        {
            internal int currentSubpath;
            internal object[] parentPartitionKeys;
            internal ContextControllerConditionNonHA startCondition;

            internal NestedEntry(
                int currentSubpath,
                ContextControllerConditionNonHA startCondition,
                object[] parentPartitionKeys)
            {
                this.currentSubpath = currentSubpath;
                this.startCondition = startCondition;
                this.parentPartitionKeys = parentPartitionKeys;
            }
        }
    }
} // end of namespace