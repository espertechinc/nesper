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
	public class ContextControllerHashSvcLevelOne : ContextControllerHashSvc {
	    private readonly static object[] EMPTY_PARENT_PARTITION_KEYS = new object[0];

	    private ContextControllerFilterEntry[] filterEntries;
	    private int[] subpathOrCPIdsPreallocate;
	    private IDictionary<int, int> optionalHashes;

	    public ContextControllerHashSvcLevelOne(bool preallocate) {
	        if (!preallocate) {
	            optionalHashes = new Dictionary<>();
	        }
	    }

	    public void MgmtCreate(IntSeqKey controllerPath, object[] parentPartitionKeys) {
	        // can ignore, parent partition keys always empty
	    }

	    public void MgmtSetFilters(IntSeqKey controllerPath, ContextControllerFilterEntry[] filterEntries) {
	        this.filterEntries = filterEntries;
	    }

	    public int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path) {
	        return subpathOrCPIdsPreallocate;
	    }

	    public void MgmtSetSubpathOrCPIdsWhenPreallocate(IntSeqKey path, int[] subpathOrCPIds) {
	        subpathOrCPIdsPreallocate = subpathOrCPIds;
	    }

	    public object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath) {
	        return EMPTY_PARENT_PARTITION_KEYS;
	    }

	    public ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath) {
	        return filterEntries;
	    }

	    public bool HashHasSeenPartition(IntSeqKey controllerPath, int value) {
	        return optionalHashes.ContainsKey(value);
	    }

	    public void HashAddPartition(IntSeqKey controllerPath, int value, int subpathIdOrCPId) {
	        optionalHashes.Put(value, subpathIdOrCPId);
	    }

	    public void HashVisit(IntSeqKey controllerPath, BiConsumer<int, int> hashAndCPId) {
	        if (optionalHashes == null) {
	            if (subpathOrCPIdsPreallocate == null) {
	                return;
	            }
	            for (int i = 0; i < subpathOrCPIdsPreallocate.Length; i++) {
	                hashAndCPId.Accept(i, subpathOrCPIdsPreallocate[i]);
	            }
	            return;
	        }

	        foreach (KeyValuePair<int, int> entry in optionalHashes) {
	            hashAndCPId.Accept(entry.Key, entry.Value);
	        }
	    }

	    public int HashGetSubpathOrCPId(IntSeqKey controllerPath, int hash) {
	        if (optionalHashes == null) {
	            if (hash >= subpathOrCPIdsPreallocate.Length) {
	                return -1;
	            }
	            return subpathOrCPIdsPreallocate[hash];
	        }

	        int? entry = optionalHashes.Get(hash);
	        return entry == null ? -1 : entry;
	    }

	    public ICollection<int> Deactivate(IntSeqKey controllerPath) {
	        if (optionalHashes == null) {
	            IList<int> ids = new List<>(subpathOrCPIdsPreallocate.Length);
	            foreach (int id in subpathOrCPIdsPreallocate) {
	                ids.Add(id);
	            }
	            return ids;
	        }

	        IList<int> ids = new List<>(optionalHashes.Values());
	        optionalHashes.Clear();
	        return ids;
	    }

	    public void Destroy() {
	        if (optionalHashes != null) {
	            optionalHashes.Clear();
	        }
	        subpathOrCPIdsPreallocate = null;
	        filterEntries = null;
	    }
	}
} // end of namespace