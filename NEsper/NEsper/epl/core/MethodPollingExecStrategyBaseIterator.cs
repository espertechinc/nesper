///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;

using XLR8.CGLib;

namespace com.espertech.esper.epl.core
{
	public abstract class MethodPollingExecStrategyBaseIterator : MethodPollingExecStrategyBase
	{
	    public MethodPollingExecStrategyBaseIterator(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, string variableName, VariableService variableService)
            : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
	    }

	    protected abstract EventBean GetEventBean(object value);

	    protected override IList<EventBean> HandleResult(object invocationResult)
	    {
	        var enumerator = invocationResult as System.Collections.IEnumerator;
	        if (enumerator == null)
	        {
	            var enumerable = invocationResult as System.Collections.IEnumerable;
	            if (enumerable == null)
	            {
	                return Collections.GetEmptyList<EventBean>();
	                //throw new ArgumentException("invalid input - not enumerable", "invocationResult");
	            }

	            enumerator = enumerable.GetEnumerator();
	        }

	        var rowResult = new List<EventBean>(2);
	        while (enumerator.MoveNext())
	        {
	            var value = enumerator.Current;
	            if (CheckNonNullArrayValue(value))
	            {
	                var @event = GetEventBean(value);
	                rowResult.Add(@event);
	            }
	        }
	        return rowResult;
	    }
	}
} // end of namespace
