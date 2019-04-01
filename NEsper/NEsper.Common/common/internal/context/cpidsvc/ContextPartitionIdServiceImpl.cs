///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.cpidsvc
{
    public class ContextPartitionIdServiceImpl : ContextPartitionIdService
    {
        private readonly IDictionary<int, object[]> cpids = new Dictionary<int, object[]>();
        private int lastAssignedId = -1;

        public void Clear()
        {
            cpids.Clear();
            lastAssignedId = -1;
        }

        public object[] GetPartitionKeys(int id)
        {
            return cpids.Get(id);
        }

        public int AllocateId(object[] partitionKeys)
        {
            while (true) {
                if (lastAssignedId < int.MaxValue) {
                    lastAssignedId++;
                }
                else {
                    lastAssignedId = 0;
                }

                if (!cpids.ContainsKey(lastAssignedId)) {
                    cpids.Put(lastAssignedId, partitionKeys);
                    return lastAssignedId;
                }
            }
        }

        public void RemoveId(int contextPartitionId)
        {
            cpids.Remove(contextPartitionId);
        }

        public ICollection<int> Ids => new List<int>(cpids.Keys);

        public void Destroy()
        {
            // no action, service discarded and GC'd
        }
    }
} // end of namespace