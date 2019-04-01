///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodConversionStrategyEventBeans : MethodConversionStrategyBase
    {
        public override IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            AgentInstanceContext agentInstanceContext)
        {
            if (invocationResult == null)
            {
                return Collections.GetEmptyList<EventBean>();
            }

            if (invocationResult.GetType().IsArray)
            {
                return invocationResult.UnwrapIntoList<EventBean>();
            }

            if (invocationResult.GetType().IsGenericCollection())
            {
                var collection = invocationResult.Unwrap<EventBean>();
                var length = collection.Count;
                if (length == 0)
                {
                    return Collections.GetEmptyList<EventBean>();
                }

                var genRowResult = new List<EventBean>(length);
                foreach (var value in collection)
                {
                    if (value != null)
                    {
                        genRowResult.Add(value);
                    }
                }

                return genRowResult;
            }

            var it = (IEnumerator<EventBean>)invocationResult;
            if (!it.MoveNext())
            {
                return Collections.GetEmptyList<EventBean>();
            }

            var rowResult = new List<EventBean>();
            for (; it.MoveNext();)
            {
                var value = it.Current;
                if (value != null)
                {
                    rowResult.Add(value);
                }
            }

            return rowResult;
        }
    }
} // end of namespace