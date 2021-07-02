///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashSvcLevelOne : ContextControllerHashSvc
    {
        private static readonly object[] EmptyParentPartitionKeys = new object[0];

        private ContextControllerFilterEntry[] _filterEntries;
        private readonly IDictionary<int, int> _optionalHashes;
        private int[] _subpathOrCpIdsPreallocate;

        public ContextControllerHashSvcLevelOne(bool preallocate)
        {
            if (!preallocate) {
                _optionalHashes = new Dictionary<int, int>();
            }
        }

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            // can ignore, parent partition keys always empty
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            this._filterEntries = filterEntries;
        }

        public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path)
        {
            return _subpathOrCpIdsPreallocate;
        }

        public void MgmtSetSubpathOrCPIdsWhenPreallocate(
            IntSeqKey path,
            int[] subpathOrCPIds)
        {
            _subpathOrCpIdsPreallocate = subpathOrCPIds;
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            return EmptyParentPartitionKeys;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return _filterEntries;
        }

        public bool HashHasSeenPartition(
            IntSeqKey controllerPath,
            int value)
        {
            return _optionalHashes.ContainsKey(value);
        }

        public void HashAddPartition(
            IntSeqKey controllerPath,
            int value,
            int subpathIdOrCPId)
        {
            _optionalHashes.Put(value, subpathIdOrCPId);
        }

        public void HashVisit(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId)
        {
            if (_optionalHashes == null) {
                if (_subpathOrCpIdsPreallocate == null) {
                    return;
                }

                for (var i = 0; i < _subpathOrCpIdsPreallocate.Length; i++) {
                    hashAndCPId.Invoke(i, _subpathOrCpIdsPreallocate[i]);
                }

                return;
            }

            foreach (var entry in _optionalHashes) {
                hashAndCPId.Invoke(entry.Key, entry.Value);
            }
        }

        public int HashGetSubpathOrCPId(
            IntSeqKey controllerPath,
            int hash)
        {
            if (_optionalHashes == null) {
                if (hash >= _subpathOrCpIdsPreallocate.Length) {
                    return -1;
                }

                return _subpathOrCpIdsPreallocate[hash];
            }

            if (_optionalHashes.TryGetValue(hash, out var entry)) {
                return entry;
            }

            return -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            if (_optionalHashes == null) {
                IList<int> idsInner = new List<int>(_subpathOrCpIdsPreallocate.Length);
                foreach (var id in _subpathOrCpIdsPreallocate) {
                    idsInner.Add(id);
                }

                return idsInner;
            }

            IList<int> ids = new List<int>(_optionalHashes.Values);
            _optionalHashes.Clear();
            return ids;
        }

        public void Destroy()
        {
            _optionalHashes?.Clear();

            _subpathOrCpIdsPreallocate = null;
            _filterEntries = null;
        }
    }
} // end of namespace