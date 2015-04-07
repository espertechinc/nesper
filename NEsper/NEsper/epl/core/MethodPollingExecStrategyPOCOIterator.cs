///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;

using XLR8.CGLib;

namespace com.espertech.esper.epl.core
{
	public class MethodPollingExecStrategyPOCOIterator : MethodPollingExecStrategyBaseIterator
	{
	    public MethodPollingExecStrategyPOCOIterator(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, string variableName, VariableService variableService)
	        : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
	    }

	    protected override EventBean GetEventBean(object value)
        {
	        return EventAdapterService.AdapterForObject(value);
	    }
	}
} // end of namespace
