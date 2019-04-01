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
using com.espertech.esper.common.@internal.context.controller.condition;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
	public interface ContextControllerKeyedSvc {
	    void MgmtCreate(IntSeqKey controllerPath, object[] parentPartitionKeys);

	    void MgmtSetFilters(IntSeqKey controllerPath, ContextControllerFilterEntry[] filterEntries);

	    int MgmtGetIncSubpath(IntSeqKey controllerPath);

	    ContextControllerFilterEntry[] MgmtGetFilters(IntSeqKey controllerPath);

	    object[] MgmtGetPartitionKeys(IntSeqKey controllerPath);

	    bool KeyHasSeen(IntSeqKey controllerPath, object key);

	    void KeyAdd(IntSeqKey controllerPath, object key, int subpathIdOrCPId, ContextControllerConditionNonHA terminationCondition);

	    ContextControllerKeyedSvcEntry KeyRemove(IntSeqKey controllerPath, object key);

	    IList<ContextControllerConditionNonHA> KeyGetTermConditions(IntSeqKey controllerPath);

	    int KeyGetSubpathOrCPId(IntSeqKey controllerPath, object key);

	    void KeyVisit(IntSeqKey controllerPath, BiConsumer<object, int> keyAndSubpathOrCPId);

	    ICollection<int> Deactivate(IntSeqKey controllerPath);

	    void Destroy();
	}
} // end of namespace