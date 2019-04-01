///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	/// <summary>
	/// Starts and provides the stop method for EPL statements.
	/// </summary>
	public class FAFQueryMethodIUDInsertInto : FAFQueryMethodIUDBase {

	    private SelectExprProcessor insertHelper;

	    public void SetInsertHelper(SelectExprProcessor insertHelper) {
	        this.insertHelper = insertHelper;
	    }

	    protected override EventBean[] Execute(FireAndForgetInstance fireAndForgetProcessorInstance) {
	        return fireAndForgetProcessorInstance.ProcessInsert(this);
	    }

	    public SelectExprProcessor InsertHelper {
	        get => insertHelper;
	    }
	}
} // end of namespace