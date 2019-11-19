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
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashSvcLevelAny : ContextControllerHashSvc
    {
        private readonly IDictionary<IntSeqKey, MgmtInfo> _mgmt = new Dictionary<IntSeqKey, MgmtInfo>();
        private readonly IDictionary<IntSeqKey, int> _optionalHashes;

        internal ContextControllerHashSvcLevelAny(bool preallocate)
        {
            if (!preallocate) {
                _optionalHashes = new Dictionary<IntSeqKey, int>();
            }
        }

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            _mgmt.Put(controllerPath, new MgmtInfo(null, parentPartitionKeys));
        }

        public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path)
        {
            return _mgmt.Get(path).SubpathOrCPIdsPreallocate;
        }

        public void MgmtSetSubpathOrCPIdsWhenPreallocate(
            IntSeqKey path,
            int[] subpathOrCPIds)
        {
            _mgmt.Get(path).SubpathOrCPIdsPreallocate = subpathOrCPIds;
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            _mgmt.Get(controllerPath).FilterEntries = filterEntries;
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            return _mgmt.Get(controllerPath).ParentPartitionKeys;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return _mgmt.Get(controllerPath).FilterEntries;
        }

        public bool HashHasSeenPartition(
            IntSeqKey controllerPath,
            int value)
        {
            return _optionalHashes.ContainsKey(controllerPath.AddToEnd(value));
        }

        public void HashAddPartition(
            IntSeqKey controllerPath,
            int value,
            int subpathIdOrCPId)
        {
            _optionalHashes.Put(controllerPath.AddToEnd(value), subpathIdOrCPId);
        }

        public void HashVisit(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId)
        {
            if (_optionalHashes == null) {
                var mgmtInfo = _mgmt.Get(controllerPath);
                if (mgmtInfo == null || mgmtInfo.SubpathOrCPIdsPreallocate == null) {
                    return;
                }

                var subpathOrCPIds = mgmtInfo.SubpathOrCPIdsPreallocate;
                for (var i = 0; i < subpathOrCPIds.Length; i++) {
                    hashAndCPId.Invoke(i, subpathOrCPIds[i]);
                }

                return;
            }

            foreach (var entry in _optionalHashes) {
                if (controllerPath.IsParentTo(entry.Key)) {
                    hashAndCPId.Invoke(entry.Key.Last, entry.Value);
                }
            }
        }

        public int HashGetSubpathOrCPId(
            IntSeqKey controllerPath,
            int hash)
        {
            if (_optionalHashes == null) {
                var mgmtInfo = _mgmt.Get(controllerPath);
                return mgmtInfo.SubpathOrCPIdsPreallocate[hash];
            }

            if (_optionalHashes.TryGetValue(controllerPath.AddToEnd(hash), out var found)) {
                return found;
            }

            return -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            var mgmtInfo = _mgmt.Delete(controllerPath);

            if (_optionalHashes == null) {
                return MgmtInfoToIds(mgmtInfo);
            }

            var result = new List<int>();
            var entries = _optionalHashes
                .Where(entry => controllerPath.IsParentTo(entry.Key))
                .ToList();

            entries.ForEach(
                entry => {
                    result.Add(entry.Value);
                    _optionalHashes.Remove(entry.Key);
                });

            return result;
        }

        public void Destroy()
        {
            _mgmt.Clear();
            if (_optionalHashes != null) {
                _optionalHashes.Clear();
            }
        }

        private ICollection<int> MgmtInfoToIds(MgmtInfo mgmtInfo)
        {
            var subpathOrCPIdsPreallocate = mgmtInfo.SubpathOrCPIdsPreallocate;
            var ids = new List<int>(subpathOrCPIdsPreallocate.Length);
            foreach (var id in subpathOrCPIdsPreallocate) {
                ids.Add(id);
            }

            return ids;
        }

        private class MgmtInfo
        {
            internal MgmtInfo(
                ContextControllerFilterEntry[] filterEntries,
                object[] parentPartitionKeys)
            {
                FilterEntries = filterEntries;
                ParentPartitionKeys = parentPartitionKeys;
            }

            internal ContextControllerFilterEntry[] FilterEntries { get; set; }

            public object[] ParentPartitionKeys { get; }

            internal int[] SubpathOrCPIdsPreallocate { get; set; }
        }
    }
} // end of namespace