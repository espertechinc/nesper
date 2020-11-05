///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public abstract class MethodConversionStrategyCollection : MethodConversionStrategyBase
    {
        protected abstract EventBean GetEventBean(
            object value,
            AgentInstanceContext agentInstanceContext);

        public override IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            AgentInstanceContext agentInstanceContext)
        {
            ICollection<object> collection = invocationResult.Unwrap<object>();
            var length = collection.Count;
            if (length == 0) {
                return Collections.GetEmptyList<EventBean>();
            }

            if (length == 1) {
                object value = collection.First();
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, agentInstanceContext);
                    return Collections.SingletonList(@event);
                }

                return Collections.GetEmptyList<EventBean>();
            }

            var rowResult = new List<EventBean>(length);
            foreach (var value in collection) {
                if (CheckNonNullArrayValue(value, origin)) {
                    var @event = GetEventBean(value, agentInstanceContext);
                    rowResult.Add(@event);
                }
            }

            return rowResult;
        }
    }
} // end of namespace