///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public abstract class MethodConversionStrategyArray : MethodConversionStrategyBase
    {
        protected abstract EventBean GetEventBean(
            object value,
            AgentInstanceContext agentInstanceContext);

        public override IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            AgentInstanceContext agentInstanceContext)
        {
            var array = (Array) invocationResult;
            var length = array.Length;
            if (length == 0) {
                return Collections.GetEmptyList<EventBean>();
            }

            if (length == 1) {
                var value = array.GetValue(0);
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, agentInstanceContext);
                    return Collections.SingletonList(@event);
                }

                return Collections.GetEmptyList<EventBean>();
            }

            var rowResult = new List<EventBean>(length);
            for (var i = 0; i < length; i++) {
                var value = array.GetValue(i);
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, agentInstanceContext);
                    rowResult.Add(@event);
                }
            }

            return rowResult;
        }
    }
} // end of namespace