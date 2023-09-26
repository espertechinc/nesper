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
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedSvcLevelOne : ContextControllerKeyedSvc
    {
        private static readonly object[] EMPTY_PARTITION_KEYS = Array.Empty<object>();

        private readonly IDictionary<object, ContextControllerKeyedSvcEntry> _keys =
            new NullableDictionary<object, ContextControllerKeyedSvcEntry>(
                new Dictionary<object, ContextControllerKeyedSvcEntry>());

        private int _currentSubpathId;
        private ContextControllerFilterEntry[] _filterEntries;

        public object[] MgmtGetPartitionKeys(IntSeqKey controllerPath)
        {
            return EMPTY_PARTITION_KEYS;
        }

        public int MgmtGetIncSubpath(IntSeqKey controllerPath)
        {
            var subpathId = _currentSubpathId;
            _currentSubpathId++;
            return subpathId;
        }

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            // no action, parent partition keys always empty
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            _filterEntries = filterEntries;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return _filterEntries;
        }

        public bool KeyHasSeen(
            IntSeqKey controllerPath,
            object key)
        {
            return _keys.ContainsKey(key);
        }

        public void KeyAdd(
            IntSeqKey controllerPath,
            object key,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA terminationCondition)
        {
            _keys.Put(key, new ContextControllerKeyedSvcEntry(subpathIdOrCPId, terminationCondition));
        }

        public ContextControllerKeyedSvcEntry KeyRemove(
            IntSeqKey controllerPath,
            object key)
        {
            return _keys.Delete(key);
        }

        public IList<ContextControllerConditionNonHA> KeyGetTermConditions(IntSeqKey controllerPath)
        {
            IList<ContextControllerConditionNonHA> conditions = new List<ContextControllerConditionNonHA>();
            foreach (var entry in _keys) {
                conditions.Add(entry.Value.TerminationCondition);
            }

            return conditions;
        }

        public void KeyVisit(
            IntSeqKey controllerPath,
            BiConsumer<object, int> keyAndSubpathOrCPId)
        {
            foreach (var entry in _keys) {
                keyAndSubpathOrCPId.Invoke(entry.Key, entry.Value.SubpathOrCPId);
            }
        }

        public void KeyVisitEntry(
            IntSeqKey controllerPath,
            Consumer<ContextControllerKeyedSvcEntry> consumer)
        {
            foreach (var entry in _keys) {
                consumer.Invoke(entry.Value);
            }
        }

        public int KeyGetSubpathOrCPId(
            IntSeqKey controllerPath,
            object key)
        {
            var entry = _keys.Get(key);
            return entry?.SubpathOrCPId ?? -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            IList<int> result = new List<int>(_keys.Count);
            foreach (var entry in _keys) {
                result.Add(entry.Value.SubpathOrCPId);
            }

            Destroy();
            return result;
        }

        public void Destroy()
        {
            _keys.Clear();
            _filterEntries = null;
        }
    }
} // end of namespace