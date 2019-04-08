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
    public class ContextControllerHashSvcLevelAny : ContextControllerHashSvc
    {
        private readonly IDictionary<IntSeqKey, MgmtInfo> mgmt = new Dictionary<IntSeqKey, MgmtInfo>();
        private readonly IDictionary<IntSeqKey, int> optionalHashes;

        internal ContextControllerHashSvcLevelAny(bool preallocate)
        {
            if (!preallocate) {
                optionalHashes = new Dictionary<IntSeqKey, int>();
            }
        }

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys)
        {
            mgmt.Put(controllerPath, new MgmtInfo(null, parentPartitionKeys));
        }

        public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path)
        {
            return mgmt.Get(path).SubpathOrCPIdsPreallocate;
        }

        public void MgmtSetSubpathOrCPIdsWhenPreallocate(
            IntSeqKey path,
            int[] subpathOrCPIds)
        {
            mgmt.Get(path).SubpathOrCPIdsPreallocate = subpathOrCPIds;
        }

        public void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries)
        {
            mgmt.Get(controllerPath).FilterEntries = filterEntries;
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            return mgmt.Get(controllerPath).ParentPartitionKeys;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return mgmt.Get(controllerPath).FilterEntries;
        }

        public bool HashHasSeenPartition(
            IntSeqKey controllerPath,
            int value)
        {
            return optionalHashes.ContainsKey(controllerPath.AddToEnd(value));
        }

        public void HashAddPartition(
            IntSeqKey controllerPath,
            int value,
            int subpathIdOrCPId)
        {
            optionalHashes.Put(controllerPath.AddToEnd(value), subpathIdOrCPId);
        }

        public void HashVisit(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId)
        {
            if (optionalHashes == null) {
                var mgmtInfo = mgmt.Get(controllerPath);
                if (mgmtInfo == null || mgmtInfo.SubpathOrCPIdsPreallocate == null) {
                    return;
                }

                var subpathOrCPIds = mgmtInfo.SubpathOrCPIdsPreallocate;
                for (var i = 0; i < subpathOrCPIds.Length; i++) {
                    hashAndCPId.Invoke(i, subpathOrCPIds[i]);
                }

                return;
            }

            foreach (var entry in optionalHashes) {
                if (controllerPath.IsParentTo(entry.Key)) {
                    hashAndCPId.Invoke(entry.Key.Last, entry.Value);
                }
            }
        }

        public int HashGetSubpathOrCPId(
            IntSeqKey controllerPath,
            int hash)
        {
            if (optionalHashes == null) {
                var mgmtInfo = mgmt.Get(controllerPath);
                return mgmtInfo.SubpathOrCPIdsPreallocate[hash];
            }

            if (optionalHashes.TryGetValue(controllerPath.AddToEnd(hash), out var found)) {
                return found;
            }

            return -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            var mgmtInfo = mgmt.Delete(controllerPath);

            if (optionalHashes == null) {
                return MgmtInfoToIds(mgmtInfo);
            }

            var it = optionalHashes.GetEnumerator();
            IList<int> result = new List<int>();
            while (it.MoveNext()) {
                var entry = it.Current;
                if (controllerPath.IsParentTo(entry.Key)) {
                    result.Add(entry.Value);
                    it.Remove();
                }
            }

            return result;
        }

        public void Destroy()
        {
            mgmt.Clear();
            if (optionalHashes != null) {
                optionalHashes.Clear();
            }
        }

        private ICollection<int> MgmtInfoToIds(MgmtInfo mgmtInfo)
        {
            var subpathOrCPIdsPreallocate = mgmtInfo.SubpathOrCPIdsPreallocate;
            IList<int> ids = new List<int>(subpathOrCPIdsPreallocate.Length);
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