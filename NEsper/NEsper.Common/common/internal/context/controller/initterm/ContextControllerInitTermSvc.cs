///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public interface ContextControllerInitTermSvc
    {
        void MgmtCreate(
            IntSeqKey controllerPath,
            object[] parentPartitionKeys);

        object[] MgmtGetParentPartitionKeys(IntSeqKey controllerPath);

        int MgmtUpdIncSubpath(IntSeqKey controllerPath);

        ContextControllerConditionNonHA MgmtUpdClearStartCondition(IntSeqKey controllerPath);

        void MgmtUpdSetStartCondition(
            IntSeqKey controllerPath,
            ContextControllerConditionNonHA startCondition);

        ContextControllerConditionNonHA MgmtDelete(IntSeqKey controllerPath);

        void EndCreate(
            IntSeqKey endConditionPath,
            int subpathIdOrCPId,
            ContextControllerConditionNonHA endCondition,
            ContextControllerInitTermPartitionKey partitionKey);

        ICollection<ContextControllerInitTermSvcEntry> EndDeleteByParentPath(IntSeqKey controllerPath);

        ContextControllerInitTermSvcEntry EndDelete(IntSeqKey conditionPath);

        void EndVisit(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerInitTermPartitionKey, int> partKeyAndCPId);

        void EndVisitConditions(
            IntSeqKey controllerPath,
            BiConsumer<ContextControllerConditionNonHA, int> partKeyAndCPId);

        void Destroy();
        
        ContextControllerCondition MgmtGetStartCondition(IntSeqKey conditionPath);
    }
} // end of namespace