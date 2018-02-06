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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    public class SubordWMatchExprLookupStrategyIndexedFiltered : SubordWMatchExprLookupStrategy
    {
        private readonly ExprEvaluator _joinExpr;
        private readonly EventBean[] _eventsPerStream;
        private readonly SubordTableLookupStrategy _tableLookupStrategy;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="joinExpr">the validated where clause of the on-delete</param>
        /// <param name="tableLookupStrategy">the strategy for looking up in an index the matching events using correlation</param>
        public SubordWMatchExprLookupStrategyIndexedFiltered(ExprEvaluator joinExpr, SubordTableLookupStrategy tableLookupStrategy)
        {
            _joinExpr = joinExpr;
            _eventsPerStream = new EventBean[2];
            _tableLookupStrategy = tableLookupStrategy;
        }

        public SubordTableLookupStrategy TableLookupStrategy => _tableLookupStrategy;

        public EventBean[] Lookup(EventBean[] newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType.INDEXED_FILTERED); }
            ISet<EventBean> foundEvents = null;

            var evaluateParams = new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext);

            // For every new event (usually 1)
            foreach (EventBean newEvent in newData)
            {
                _eventsPerStream[1] = newEvent;
    
                // use index to find match
                var matches = _tableLookupStrategy.Lookup(_eventsPerStream, exprEvaluatorContext);
                if ((matches == null) || (matches.IsEmpty()))
                {
                    continue;
                }

                // evaluate expression
                var eventsIt = matches.GetEnumerator();
                while (eventsIt.MoveNext())
                {
                    _eventsPerStream[0] = eventsIt.Current;
    
                    foreach (EventBean aNewData in newData)
                    {
                        _eventsPerStream[1] = aNewData;    // Stream 1 events are the originating events (on-delete events)

                        var result = (bool?) _joinExpr.Evaluate(evaluateParams);
                        if (result != null)
                        {
                            if (result.Value)
                            {
                                if (foundEvents == null)
                                {
                                    foundEvents = new LinkedHashSet<EventBean>();
                                }
                                foundEvents.Add(_eventsPerStream[0]);
                            }
                        }
                    }
                }
            }
    
            if (foundEvents == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(null); }
                return null;
            }
    
            EventBean[] events = foundEvents.ToArray();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(events); }
    
            return events;
        }
    
        public override string ToString() {
            return ToQueryPlan();
        }
    
        public string ToQueryPlan() {
            return GetType().Name + " " + " strategy " + _tableLookupStrategy.ToQueryPlan();
        }
    }
}
