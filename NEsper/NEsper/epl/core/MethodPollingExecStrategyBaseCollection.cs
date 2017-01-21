///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;

using XLR8.CGLib;

namespace com.espertech.esper.epl.core
{
	public abstract class MethodPollingExecStrategyBaseCollection : MethodPollingExecStrategyBase
	{
	    public MethodPollingExecStrategyBaseCollection(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, string variableName, VariableService variableService)
	        : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
	    }

	    protected abstract EventBean GetEventBean(object value);

	    protected override IList<EventBean> HandleResult(object invocationResult)
        {
	        var collection = invocationResult.Unwrap<object>();
	        var length = collection.Count;
	        if (length == 0) {
	            return Collections.GetEmptyList<EventBean>();
	        }
	        if (length == 1) {
	            var value = collection.First();
	            if (CheckNonNullArrayValue(value)) {
	                var @event = GetEventBean(value);
	                return Collections.SingletonList(@event);
	            }
	            return Collections.GetEmptyList<EventBean>();
	        }
	        var rowResult = new List<EventBean>(length);
	        var it = collection.GetEnumerator();
	        while (it.MoveNext()) {
	            object value = it.Current;
	            if (CheckNonNullArrayValue(value)) {
	                var @event = GetEventBean(value);
	                rowResult.Add(@event);
	            }
	        }
	        return rowResult;
	    }
	}
} // end of namespace
