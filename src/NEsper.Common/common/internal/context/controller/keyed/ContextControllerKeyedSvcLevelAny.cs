///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedSvcLevelAny : ContextControllerKeyedSvc
    {
        private readonly IDictionary<ContextControllerKeyedCompositeKey, ContextControllerKeyedSvcEntry> _keys =
            new Dictionary<ContextControllerKeyedCompositeKey, ContextControllerKeyedSvcEntry>();

        private readonly IDictionary<IntSeqKey, MgmtInfo> _mgmt = new Dictionary<IntSeqKey, MgmtInfo>();

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            _mgmt.Put(controllerPath, new MgmtInfo(0, null, parentPartitionKeys));
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            var entry = _mgmt.Get(controllerPath);
            entry.FilterEntries = filterEntries;
        }

        public object[] MgmtGetPartitionKeys(IntSeqKey controllerPath)
        {
            return _mgmt.Get(controllerPath).parentPartitionKeys;
        }

        public int MgmtGetIncSubpath(IntSeqKey controllerPath)
        {
            var entry = _mgmt.Get(controllerPath);
            var subpathId = entry.currentSubpathId;
            entry.currentSubpathId++;
            return subpathId;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return _mgmt.Get(controllerPath).filterEntries;
        }

        public bool KeyHasSeen(
            IntSeqKey controllerPath,
            object key)
        {
            return _keys.ContainsKey(new ContextControllerKeyedCompositeKey(controllerPath, key));
        }

        public void KeyAdd(
            IntSeqKey controllerPath,
            object key,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA terminationCondition)
        {
            _keys.Put(
                new ContextControllerKeyedCompositeKey(controllerPath, key),
                new ContextControllerKeyedSvcEntry(subpathIdOrCPId, terminationCondition));
        }

        public ContextControllerKeyedSvcEntry KeyRemove(
            IntSeqKey controllerPath,
            object key)
        {
            return _keys.Delete(new ContextControllerKeyedCompositeKey(controllerPath, key));
        }

        public IList<ContextControllerConditionNonHA> KeyGetTermConditions(IntSeqKey controllerPath)
        {
            IList<ContextControllerConditionNonHA> conditions = new List<ContextControllerConditionNonHA>();
            foreach (var entry in _keys) {
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
            foreach (var entry in _keys) {
                if (controllerPath.Equals(entry.Key.Path)) {
                    keyAndSubpathOrCPId.Invoke(entry.Key.Key, entry.Value.SubpathOrCPId);
                }
            }
        }

        public void KeyVisitEntry(
            IntSeqKey controllerPath,
            Consumer<ContextControllerKeyedSvcEntry> consumer)
        {
            foreach (var entry in _keys) {
                if (controllerPath.Equals(entry.Key.Path)) {
                    consumer.Invoke(entry.Value);
                }
            }
        }

        public int KeyGetSubpathOrCPId(
            IntSeqKey controllerPath,
            object key)
        {
            var entry = _keys.Get(new ContextControllerKeyedCompositeKey(controllerPath, key));
            return entry?.SubpathOrCPId ?? -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            var entriesToRemove = _keys
                .Where(entry => controllerPath.Equals(entry.Key.Path))
                .ToList();

            var ids = entriesToRemove
                .Select(entry => entry.Value.SubpathOrCPId)
                .ToList();

            entriesToRemove.ForEach(entry => _keys.Remove(entry.Key));

            return ids;
        }

        public void Destroy()
        {
            _mgmt.Clear();
            _keys.Clear();
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