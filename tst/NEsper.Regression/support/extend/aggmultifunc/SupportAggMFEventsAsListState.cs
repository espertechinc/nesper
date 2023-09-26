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

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFEventsAsListState : AggregationMultiFunctionState
    {
        public IList<SupportBean> Events { get; }

        public SupportAggMFEventsAsListState()
        {
            Events = new List<SupportBean>();
        }

        public SupportAggMFEventsAsListState(IList<SupportBean> events)
        {
            Events = events;
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Events.Add((SupportBean) eventsPerStream[0].Underlying);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Events.Remove((SupportBean) eventsPerStream[0].Underlying);
        }

        public void Clear()
        {
            Events.Clear();
        }
    }
} // end of namespace