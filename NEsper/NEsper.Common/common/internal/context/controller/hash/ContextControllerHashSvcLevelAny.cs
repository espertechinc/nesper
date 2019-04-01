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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
	public class ContextControllerHashSvcLevelAny : ContextControllerHashSvc {
	    private readonly IDictionary<IntSeqKey, MgmtInfo> mgmt = new Dictionary<IntSeqKey, MgmtInfo>();
	    private IDictionary<IntSeqKey, int> optionalHashes;

	    ContextControllerHashSvcLevelAny(bool preallocate) {
	        if (!preallocate) {
	            optionalHashes = new Dictionary<IntSeqKey, int>();
	        }
	    }

	    public void MgmtCreate(IntSeqKey controllerPath, object[] parentPartitionKeys) {
	        mgmt.Put(controllerPath, new MgmtInfo(null, parentPartitionKeys));
	    }

	    public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path) {
	        return mgmt.Get(path).SubpathOrCPIdsPreallocate;
	    }

	    public void MgmtSetSubpathOrCPIdsWhenPreallocate(IntSeqKey path, int[] subpathOrCPIds) {
	        mgmt.Get(path).SubpathOrCPIdsPreallocate = subpathOrCPIds;
	    }

	    public void MgmtSetFilters(IntSeqKey controllerPath, ContextControllerFilterEntry[] filterEntries) {
	        mgmt.Get(controllerPath).FilterEntries = filterEntries;
	    }

	    public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath) {
	        return mgmt.Get(controllerPath).ParentPartitionKeys;
	    }

	    public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath) {
	        return mgmt.Get(controllerPath).FilterEntries;
	    }

	    public bool HashHasSeenPartition(IntSeqKey controllerPath, int value) {
	        return optionalHashes.ContainsKey(controllerPath.AddToEnd(value));
	    }

	    public void HashAddPartition(IntSeqKey controllerPath, int value, int subpathIdOrCPId) {
	        optionalHashes.Put(controllerPath.AddToEnd(value), subpathIdOrCPId);
	    }

	    public void HashVisit(IntSeqKey controllerPath, BiConsumer<int, int> hashAndCPId) {
	        if (optionalHashes == null) {
	            MgmtInfo mgmtInfo = mgmt.Get(controllerPath);
	            if (mgmtInfo == null || mgmtInfo.SubpathOrCPIdsPreallocate == null) {
	                return;
	            }
	            int[] subpathOrCPIds = mgmtInfo.SubpathOrCPIdsPreallocate;
	            for (int i = 0; i < subpathOrCPIds.Length; i++) {
	                hashAndCPId.Accept(i, subpathOrCPIds[i]);
	            }
	            return;
	        }

	        foreach (KeyValuePair<IntSeqKey, int> entry in optionalHashes) {
	            if (controllerPath.IsParentTo(entry.Key)) {
	                hashAndCPId.Accept(entry.Key.Last(), entry.Value);
	            }
	        }
	    }

	    public int HashGetSubpathOrCPId(IntSeqKey controllerPath, int hash) {
	        if (optionalHashes == null) {
	            MgmtInfo mgmtInfo = mgmt.Get(controllerPath);
	            return mgmtInfo.SubpathOrCPIdsPreallocate[hash];
	        }

	        int? found = optionalHashes.Get(controllerPath.AddToEnd(hash));
	        return found == null ? -1 : found;
	    }

	    public ICollection<int> Deactivate(IntSeqKey controllerPath) {
	        MgmtInfo mgmtInfo = mgmt.Remove(controllerPath);

	        if (optionalHashes == null) {
	            return MgmtInfoToIds(mgmtInfo);
	        }

	        IEnumerator<KeyValuePair<IntSeqKey, int>> it = optionalHashes.GetEnumerator();
	        IList<int> result = new List<>();
	        while (it.MoveNext()) {
	            KeyValuePair<IntSeqKey, int> entry = it.Current;
	            if (controllerPath.IsParentTo(entry.Key)) {
	                result.Add(entry.Value);
	                it.Remove();
	            }
	        }
	        return result;
	    }

	    public void Destroy() {
	        mgmt.Clear();
	        if (optionalHashes != null) {
	            optionalHashes.Clear();
	        }
	    }

	    private ICollection<int> MgmtInfoToIds(MgmtInfo mgmtInfo) {
	        int[] subpathOrCPIdsPreallocate = mgmtInfo.SubpathOrCPIdsPreallocate;
	        IList<int> ids = new List<>(subpathOrCPIdsPreallocate.Length);
	        foreach (int id in subpathOrCPIdsPreallocate) {
	            ids.Add(id);
	        }
	        return ids;
	    }

	    private class MgmtInfo {
	        private ContextControllerFilterEntry[] filterEntries;
	        private object[] parentPartitionKeys;
	        private int[] subpathOrCPIdsPreallocate;

	        MgmtInfo(ContextControllerFilterEntry[] filterEntries, object[] parentPartitionKeys) {
	            this.filterEntries = filterEntries;
	            this.parentPartitionKeys = parentPartitionKeys;
	        }

	        ContextControllerFilterEntry[] GetFilterEntries() {
	            return filterEntries;
	        }

	        public object[] ParentPartitionKeys
	        {
	            get => parentPartitionKeys;
	        }

	        void SetFilterEntries(ContextControllerFilterEntry[] filterEntries) {
	            this.filterEntries = filterEntries;
	        }

	        int[] GetSubpathOrCPIdsPreallocate() {
	            return subpathOrCPIdsPreallocate;
	        }

	        void SetSubpathOrCPIdsPreallocate(int[] subpathOrCPIdsPreallocate) {
	            this.subpathOrCPIdsPreallocate = subpathOrCPIdsPreallocate;
	        }
	    }
	}
} // end of namespace