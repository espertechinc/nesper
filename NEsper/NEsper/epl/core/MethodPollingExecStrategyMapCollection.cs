///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;

using XLR8.CGLib;

namespace com.espertech.esper.epl.core
{
	public class MethodPollingExecStrategyMapCollection : MethodPollingExecStrategyBaseCollection
	{
	    public MethodPollingExecStrategyMapCollection(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, string variableName, VariableService variableService)
            : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
	    }

	    protected override EventBean GetEventBean(object value)
        {
            var valueDataMap = value.UnwrapStringDictionary();
	        return EventAdapterService.AdapterForTypedMap(valueDataMap, EventType);
	    }
	}
} // end of namespace
