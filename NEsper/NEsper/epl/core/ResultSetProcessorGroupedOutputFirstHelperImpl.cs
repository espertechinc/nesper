///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.view;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorGroupedOutputFirstHelperImpl : ResultSetProcessorGroupedOutputFirstHelper
	{
	    private readonly IDictionary<object, OutputConditionPolled> outputState =
	        new Dictionary<object, OutputConditionPolled>().WithNullSupport();

	    public void Remove(object key) {
	        outputState.Remove(key);
	    }

	    public OutputConditionPolled GetOrAllocate(object mk, AgentInstanceContext agentInstanceContext, OutputConditionPolledFactory factory)
	    {
	        OutputConditionPolled outputStateGroup = outputState.Get(mk);
	        if (outputStateGroup == null) {
	            outputStateGroup = factory.MakeNew(agentInstanceContext);
	            outputState.Put(mk, outputStateGroup);
	        }
	        return outputStateGroup;
	    }

	    public OutputConditionPolled Get(object mk) {
	        return outputState.Get(mk);
	    }

	    public void Put(object mk, OutputConditionPolled outputStateGroup) {
	        outputState.Put(mk, outputStateGroup);
	    }

	    public void Destroy() {
	        // no action required
	    }
	}
} // end of namespace
