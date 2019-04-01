///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedSvcLevelAny : ContextControllerKeyedSvc
    {
        private readonly IDictionary<ContextControllerKeyedCompositeKey, ContextControllerKeyedSvcEntry> keys =
            new Dictionary<ContextControllerKeyedCompositeKey, ContextControllerKeyedSvcEntry>();

        private readonly IDictionary<IntSeqKey, MgmtInfo> mgmt = new Dictionary<IntSeqKey, MgmtInfo>();

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            mgmt.Put(controllerPath, new MgmtInfo(0, null, parentPartitionKeys));
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            var entry = mgmt.Get(controllerPath);
            entry.FilterEntries = filterEntries;
        }

        public object[] MgmtGetPartitionKeys(IntSeqKey controllerPath)
        {
            return mgmt.Get(controllerPath).parentPartitionKeys;
        }

        public int MgmtGetIncSubpath(IntSeqKey controllerPath)
        {
            var entry = mgmt.Get(controllerPath);
            var subpathId = entry.currentSubpathId;
            entry.currentSubpathId++;
            return subpathId;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return mgmt.Get(controllerPath).filterEntries;
        }

        public bool KeyHasSeen(
            IntSeqKey controllerPath,
            object key)
        {
            return keys.ContainsKey(new ContextControllerKeyedCompositeKey(controllerPath, key));
        }

        public void KeyAdd(
            IntSeqKey controllerPath,
            object key,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA terminationCondition)
        {
            keys.Put(
                new ContextControllerKeyedCompositeKey(controllerPath, key),
                new ContextControllerKeyedSvcEntry(subpathIdOrCPId, terminationCondition));
        }

        public ContextControllerKeyedSvcEntry KeyRemove(
            IntSeqKey controllerPath,
            object key)
        {
            return keys.Delete(new ContextControllerKeyedCompositeKey(controllerPath, key));
        }

        public IList<ContextControllerConditionNonHA> KeyGetTermConditions(IntSeqKey controllerPath)
        {
            IList<ContextControllerConditionNonHA> conditions = new List<ContextControllerConditionNonHA>();
            foreach (var entry in keys) {
                if (controllerPath.Equals(entry.Key.Path)) {
                    conditions.Add(entry.Value.TerminationCondition);
                }
            }

            return conditions;
        }

        public void KeyVisit(
            IntSeqKey controllerPath,
            BiConsumer<object, int> keyAndSubpathOrCPId)
        {
            foreach (var entry in keys) {
                if (controllerPath.Equals(entry.Key.Path)) {
                    keyAndSubpathOrCPId.Invoke(entry.Key.Key, entry.Value.SubpathOrCPId);
                }
            }
        }

        public int KeyGetSubpathOrCPId(
            IntSeqKey controllerPath,
            object key)
        {
            var entry = keys.Get(new ContextControllerKeyedCompositeKey(controllerPath, key));
            return entry == null ? -1 : entry.SubpathOrCPId;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            IList<int> ids = new List<int>();
            var iterator = keys.GetEnumerator();
            while (iterator.MoveNext()) {
                KeyValuePair<ContextControllerKeyedCompositeKey, ContextControllerKeyedSvcEntry> entry = iterator.Next();
                if (controllerPath.Equals(entry.Key.Path)) {
                    ids.Add(entry.Value.SubpathOrCPId);
                    iterator.Remove();
                }
            }

            return ids;
        }

        public void Destroy()
        {
            mgmt.Clear();
            keys.Clear();
        }

        internal class MgmtInfo
        {
            internal int currentSubpathId;
            internal ContextControllerFilterEntry[] filterEntries;
            internal object[] parentPartitionKeys;

            public MgmtInfo(
                int currentSubpathId,
                ContextControllerFilterEntry[] filterEntries,
                object[] parentPartitionKeys)
            {
                this.currentSubpathId = currentSubpathId;
                this.filterEntries = filterEntries;
                this.parentPartitionKeys = parentPartitionKeys;
            }

            public int CurrentSubpathId => currentSubpathId;

            public ContextControllerFilterEntry[] FilterEntries {
                get => filterEntries;
                set => filterEntries = value;
            }

            public object[] ParentPartitionKeys => parentPartitionKeys;
        }
    }
} // end of namespace