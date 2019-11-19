///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategorySvcLevelAny : ContextControllerCategorySvc
    {
        private readonly IDictionary<IntSeqKey, ContextControllerCategorySvcLevelAnyEntry> mgmt =
            new Dictionary<IntSeqKey, ContextControllerCategorySvcLevelAnyEntry>();

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys,
            int[] subpathOrCPId)
        {
            ContextControllerCategorySvcLevelAnyEntry existing = mgmt.PutIfAbsent(
                controllerPath,
                new ContextControllerCategorySvcLevelAnyEntry(parentPartitionKeys, subpathOrCPId));
            if (existing != null) {
                throw new IllegalStateException("Existing entry found");
            }
        }

        public int[] MgmtGetSubpathOrCPIds(IntSeqKey controllerPath)
        {
            var existing = mgmt.Get(controllerPath);
            return existing == null ? null : existing.SubpathOrCPids;
        }

        public int[] MgmtDelete(IntSeqKey controllerPath)
        {
            return mgmt.TryRemove(controllerPath, out ContextControllerCategorySvcLevelAnyEntry entry)
                ? entry.SubpathOrCPids
                : null;
        }

        public void Destroy()
        {
            mgmt.Clear();
        }

        private class ContextControllerCategorySvcLevelAnyEntry
        {
            public ContextControllerCategorySvcLevelAnyEntry(
                object[] parentPartitionKeys,
                int[] subpathOrCPids)
            {
                ParentPartitionKeys = parentPartitionKeys;
                SubpathOrCPids = subpathOrCPids;
            }

            public object[] ParentPartitionKeys { get; }

            public int[] SubpathOrCPids { get; }
        }
    }
} // end of namespace