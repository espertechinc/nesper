///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public interface ContextControllerCategorySvc
    {
        void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys,
            int[] subpathOrCPId);

        int[] MgmtDelete(IntSeqKey controllerPath);

        int[] MgmtGetSubpathOrCPIds(IntSeqKey controllerPath);

        void Destroy();
    }
} // end of namespace