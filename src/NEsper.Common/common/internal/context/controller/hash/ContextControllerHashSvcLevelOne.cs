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
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashSvcLevelOne : ContextControllerHashSvc
    {
        private static readonly object[] EMPTY_PARENT_PARTITION_KEYS = Array.Empty<object>();

        private ContextControllerFilterEntry[] filterEntries;
        private readonly IDictionary<int, int> optionalHashes;
        private int[] subpathOrCPIdsPreallocate;

        public ContextControllerHashSvcLevelOne(bool preallocate)
        {
            if (!preallocate) {
                optionalHashes = new Dictionary<int, int>();
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
            this.filterEntries = filterEntries;
        }

        public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path)
        {
            return subpathOrCPIdsPreallocate;
        }

        public void MgmtSetSubpathOrCPIdsWhenPreallocate(
            IntSeqKey path,
            int[] subpathOrCPIds)
        {
            subpathOrCPIdsPreallocate = subpathOrCPIds;
        }

        public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath)
        {
            return EMPTY_PARENT_PARTITION_KEYS;
        }

        public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath)
        {
            return filterEntries;
        }

        public bool HashHasSeenPartition(
            IntSeqKey controllerPath,
            int value)
        {
            return optionalHashes.ContainsKey(value);
        }

        public void HashAddPartition(
            IntSeqKey controllerPath,
            int value,
            int subpathIdOrCPId)
        {
            optionalHashes.Put(value, subpathIdOrCPId);
        }

        public void HashVisit(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId)
        {
            if (optionalHashes == null) {
                if (subpathOrCPIdsPreallocate == null) {
                    return;
                }

                for (var i = 0; i < subpathOrCPIdsPreallocate.Length; i++) {
                    hashAndCPId.Invoke(i, subpathOrCPIdsPreallocate[i]);
                }

                return;
            }

            foreach (var entry in optionalHashes) {
                hashAndCPId.Invoke(entry.Key, entry.Value);
            }
        }

        public int HashGetSubpathOrCPId(
            IntSeqKey controllerPath,
            int hash)
        {
            if (optionalHashes == null) {
                if (hash >= subpathOrCPIdsPreallocate.Length) {
                    return -1;
                }

                return subpathOrCPIdsPreallocate[hash];
            }

            if (optionalHashes.TryGetValue(hash, out var entry)) {
                return entry;
            }

            return -1;
        }

        public ICollection<int> Deactivate(IntSeqKey controllerPath)
        {
            if (optionalHashes == null) {
                IList<int> idsInner = new List<int>(subpathOrCPIdsPreallocate.Length);
                foreach (var id in subpathOrCPIdsPreallocate) {
                    idsInner.Add(id);
                }

                return idsInner;
            }

            IList<int> ids = new List<int>(optionalHashes.Values);
            optionalHashes.Clear();
            return ids;
        }

        public void Destroy()
        {
            optionalHashes?.Clear();

            subpathOrCPIdsPreallocate = null;
            filterEntries = null;
        }
    }
} // end of namespace