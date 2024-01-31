///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.category
{
    public class ContextControllerCategorySvcLevelOne : ContextControllerCategorySvc
    {
        private static readonly object[] EMPTY_PARENT_PARTITION_KEYS = Array.Empty<object>();

        private int[] subpathOrCPId;

        public void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys,
            int[] subpathOrCPId)
        {
            this.subpathOrCPId = subpathOrCPId;
        }

        public int[] MgmtGetSubpathOrCPIds(IntSeqKey controllerPath)
        {
            return subpathOrCPId;
        }

        public int[] MgmtDelete(IntSeqKey controllerPath)
        {
            var tmp = subpathOrCPId;
            subpathOrCPId = null;
            return tmp;
        }

        public void Destroy()
        {
            subpathOrCPId = null;
        }
    }
} // end of namespace