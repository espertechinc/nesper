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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    public class SubordWMatchExprLookupStrategyAllFiltered : SubordWMatchExprLookupStrategy
    {
        private readonly ExprEvaluator _joinExpr;
        private readonly EventBean[] _eventsPerStream;
        private readonly IEnumerable<EventBean> _iterableEvents;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="joinExpr">is the where clause</param>
        /// <param name="iterable">The iterable.</param>
        public SubordWMatchExprLookupStrategyAllFiltered(ExprEvaluator joinExpr, IEnumerable<EventBean> iterable)
        {
            _joinExpr = joinExpr;
            _eventsPerStream = new EventBean[2];
            _iterableEvents = iterable;
        }
    
        public EventBean[] Lookup(EventBean[] newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType.FULLTABLESCAN_FILTERED); }

            var evaluateParams = new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext);

            ISet<EventBean> removeEvents = null;
            IEnumerator<EventBean> eventsIt = _iterableEvents.GetEnumerator();
            for (;eventsIt.MoveNext();)
            {
                _eventsPerStream[0] = eventsIt.Current;
    
                foreach (EventBean aNewData in newData)
                {
                    _eventsPerStream[1] = aNewData;    // Stream 1 events are the originating events (on-delete events)

                    var resultX = _joinExpr.Evaluate(evaluateParams);
                    if (resultX != null)
                    {
                        if (true.Equals(resultX))
                        {
                            if (removeEvents == null)
                            {
                                removeEvents = new LinkedHashSet<EventBean>();
                            }
                            removeEvents.Add(_eventsPerStream[0]);
                        }
                    }
                }
            }
    
            if (removeEvents == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(null); }
                return null;
            }
    
            EventBean[] result = removeEvents.ToArray();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(result); }
            return result;
        }
    
        public override string ToString()
        {
            return ToQueryPlan();
        }
    
        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
