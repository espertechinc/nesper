///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFStateEnumerableEvents : AggregationState
    {
        private readonly IList<EventBean> _events = new List<EventBean>();
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            _events.Add(eventsPerStream[0]);
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }
    
        public void Clear()
        {
            _events.Clear();
        }

        public int Count
        {
            get { return _events.Count; }
        }

        public IList<EventBean> Events
        {
            get { return _events; }
        }

        public object EventsAsUnderlyingArray
        {
            get { return _events.Select(e => e.Underlying).ToArray(); }
        }
    }
}
