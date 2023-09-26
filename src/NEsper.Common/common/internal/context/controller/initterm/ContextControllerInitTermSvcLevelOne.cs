///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermSvcLevelOne : ContextControllerInitTermSvc
    {
        private static readonly object[] EMPTY_PARENT_PARTITION_KEYS = Array.Empty<object>();

        private int currentSubpath;

        private IDictionary<int, ContextControllerInitTermSvcEntry> endConditions =
            new Dictionary<int, ContextControllerInitTermSvcEntry>();

        private ContextControllerConditionNonHA startCondition;

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            // non-nested we do no care
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            return EMPTY_PARENT_PARTITION_KEYS;
        }

        public ContextControllerConditionNonHA MgmtDelete(IntSeqKey controllerPath)
        {
            var tmp = startCondition;
            startCondition = null;
            return tmp;
        }

        public ContextControllerConditionNonHA MgmtUpdClearStartCondition(IntSeqKey controllerPath)
        {
            var tmp = startCondition;
            startCondition = null;
            return tmp;
        }

        public void MgmtUpdSetStartCondition(
            IntSeqKey controllerPath,
            ContextControllerConditionNonHA startCondition)
        {
            this.startCondition = startCondition;
        }

        public int MgmtUpdIncSubpath(IntSeqKey controllerPath)
        {
            return currentSubpath++;
        }

        public ContextControllerCondition MgmtGetStartCondition(IntSeqKey conditionPath)
        {
            return startCondition;
        }

        public void EndCreate(
            IntSeqKey endConditionPath,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA endCondition,
            ContextControllerInitTermPartitionKey partitionKey)
        {
            endConditions.Put(
                ((IntSeqKeyOne)endConditionPath).One,
                new ContextControllerInitTermSvcEntry(subpathIdOrCPId, endCondition, partitionKey));
        }

        public ContextControllerInitTermSvcEntry EndDelete(IntSeqKey conditionPath)
        {
            return endConditions.Delete(((IntSeqKeyOne)conditionPath).One);
        }

        public ICollection<ContextControllerInitTermSvcEntry> EndDeleteByParentPath(IntSeqKey controllerPath)
        {
            IList<ContextControllerInitTermSvcEntry> entries =
                new List<ContextControllerInitTermSvcEntry>(endConditions.Values);
            endConditions.Clear();
            return entries;
        }

        public void EndVisit(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId)
        {
            foreach (var entry in endConditions) {
                partKeyAndCPId.Invoke(entry.Value.PartitionKey, entry.Value.SubpathIdOrCPId);
            }
        }

        public void EndVisitConditions(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerConditionNonHA, int> partKeyAndCPId)
        {
            foreach (var entry in endConditions) {
                partKeyAndCPId.Invoke(entry.Value.TerminationCondition, entry.Value.SubpathIdOrCPId);
            }
        }

        public void Destroy()
        {
            currentSubpath = 0;
            startCondition = null;
            endConditions = null;
        }
    }
} // end of namespace