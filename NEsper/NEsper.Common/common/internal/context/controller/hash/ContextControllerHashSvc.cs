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
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public interface ContextControllerHashSvc
    {
        void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys);

        int[] MgmtGetSubpathOrCPIdsWhenPreallocate(IntSeqKey path);

        object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath);

        ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath);

        void MgmtSetSubpathOrCPIdsWhenPreallocate(
            IntSeqKey path,
            int[] subpathOrCPIds);

        void MgmtSetFilters(
            IntSeqKey controllerPath,
            ContextControllerFilterEntry[] filterEntries);

        bool HashHasSeenPartition(
            IntSeqKey controllerPath,
            int value);

        void HashAddPartition(
            IntSeqKey controllerPath,
            int value,
            int subpathIdOrCPId);

        void HashVisit(
            IntSeqKey controllerPath,
            BiConsumer<int, int> hashAndCPId);

        int HashGetSubpathOrCPId(
            IntSeqKey controllerPath,
            int hash);

        ICollection<int> Deactivate(IntSeqKey controllerPath);

        void Destroy();
    }
} // end of namespace