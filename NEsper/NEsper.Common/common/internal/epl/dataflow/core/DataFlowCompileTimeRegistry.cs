///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.core
{
	public class DataFlowCompileTimeRegistry {
	    private ISet<string> dataFlows;

	    public void NewDataFlow(string dataFlowName) {
	        if (dataFlows == null) {
	            dataFlows = new HashSet<>();
	        }
	        if (dataFlows.Contains(dataFlowName)) {
	            throw new ExprValidationException("A dataflow by name '" + dataFlowName + "' has already been declared");
	        }
	        dataFlows.Add(dataFlowName);
	    }
	}
} // end of namespace