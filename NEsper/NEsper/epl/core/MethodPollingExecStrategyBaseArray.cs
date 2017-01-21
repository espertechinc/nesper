///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.variable;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
    public abstract class MethodPollingExecStrategyBaseArray : MethodPollingExecStrategyBase
    {
        protected MethodPollingExecStrategyBaseArray(EventAdapterService eventAdapterService, FastMethod method, EventType eventType, Object invocationTarget, MethodPollingExecStrategyEnum strategy, VariableReader variableReader, String variableName, VariableService variableService)
            : base(eventAdapterService, method, eventType, invocationTarget, strategy, variableReader, variableName, variableService)
        {
        }

    
        protected abstract EventBean GetEventBean(Object value);
    
        protected override IList<EventBean> HandleResult(Object invocationResult)
        {
            var array = (Array) invocationResult;

            int length = array.Length;
            if (length == 0) {
                return Collections.GetEmptyList<EventBean>();
            }
            if (length == 1) {
                var value = array.GetValue(0);
                if (CheckNonNullArrayValue(value)) {
                    var @event = GetEventBean(value);
                    return Collections.SingletonList(@event);
                }
                return Collections.GetEmptyList<EventBean>();
            }
            
            var rowResult = new List<EventBean>(length);
            for (int i = 0; i < length; i++)
            {
                var value = array.GetValue(i);
                if (CheckNonNullArrayValue(value)) {
                    EventBean @event = GetEventBean(value);
                    rowResult.Add(@event);
                }
            }
            return rowResult;
        }
    }
}
