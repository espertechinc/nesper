///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFMultiRTEnumerableEventsState : AggregationMultiFunctionState
    {
        public IList<EventBean> Events { get; } = new List<EventBean>();

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Events.Add(eventsPerStream[0]);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }

        public void Clear()
        {
            Events.Clear();
        }

        public int Size()
        {
            return Events.Count;
        }

        public object GetEventsAsUnderlyingArray()
        {
            var array = new SupportBean[Events.Count];

            var it = Events.GetEnumerator();
            var count = 0;
            for (; it.MoveNext();) {
                var bean = it.Advance();
                array[count++] = (SupportBean) bean.Underlying;
            }

            return array;
        }
    }
} // end of namespace