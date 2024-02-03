///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.support;

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
            if (Events != null) {
                var array = new SupportBean[Events.Count];

                using (var enumerator = Events.GetEnumerator()) {
                    for (var count = 0; enumerator.MoveNext(); count++) {
                        array[count] = (SupportBean) enumerator.Current?.Underlying;
                    }
                }

                return array;
            }

            return Array.Empty<SupportBean>();
        }
    }
} // end of namespace