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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyAllFiltered : SubordWMatchExprLookupStrategy
    {
        private readonly ExprEvaluator joinExpr;
        private readonly EventBean[] eventsPerStream;
        private readonly IEnumerable<EventBean> iterableEvents;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="joinExpr">is the where clause</param>
        /// <param name="iterable">iterable</param>
        public SubordWMatchExprLookupStrategyAllFiltered(
            ExprEvaluator joinExpr,
            IEnumerable<EventBean> iterable)
        {
            this.joinExpr = joinExpr;
            eventsPerStream = new EventBean[2];
            iterableEvents = iterable;
        }

        public EventBean[] Lookup(
            EventBean[] newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QInfraTriggeredLookup("fulltablescan_filtered");

            ISet<EventBean> removeEvents = null;

            var eventsIt = iterableEvents.GetEnumerator();
            while (eventsIt.MoveNext()) {
                eventsPerStream[0] = eventsIt.Current;

                foreach (var aNewData in newData) {
                    eventsPerStream[1] = aNewData; // Stream 1 events are the originating events (on-delete events)

                    var booleanResult = joinExpr.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    if (booleanResult != null) {
                        if (true.Equals(booleanResult)) {
                            if (removeEvents == null) {
                                removeEvents = new LinkedHashSet<EventBean>();
                            }

                            removeEvents.Add(eventsPerStream[0]);
                        }
                    }
                }
            }

            if (removeEvents == null) {
                exprEvaluatorContext.InstrumentationProvider.AInfraTriggeredLookup(null);
                return null;
            }

            var result = removeEvents.ToArray();
            exprEvaluatorContext.InstrumentationProvider.AInfraTriggeredLookup(result);
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
} // end of namespace