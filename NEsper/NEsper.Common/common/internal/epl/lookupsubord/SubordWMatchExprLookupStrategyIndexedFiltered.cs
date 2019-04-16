///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyIndexedFiltered : SubordWMatchExprLookupStrategy
    {
        private readonly ExprEvaluator joinExpr;
        private readonly EventBean[] eventsPerStream;
        private readonly SubordTableLookupStrategy tableLookupStrategy;

        public SubordWMatchExprLookupStrategyIndexedFiltered(
            ExprEvaluator joinExpr,
            SubordTableLookupStrategy tableLookupStrategy)
        {
            this.joinExpr = joinExpr;
            this.eventsPerStream = new EventBean[2];
            this.tableLookupStrategy = tableLookupStrategy;
        }

        public SubordTableLookupStrategy TableLookupStrategy {
            get => tableLookupStrategy;
        }

        public EventBean[] Lookup(
            EventBean[] newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QInfraTriggeredLookup("indexed_filtered");
            ISet<EventBean> foundEvents = null;

            // For every new event (usually 1)
            foreach (EventBean newEvent in newData) {
                eventsPerStream[1] = newEvent;

                // use index to find match
                ICollection<EventBean> matches = tableLookupStrategy.Lookup(eventsPerStream, exprEvaluatorContext);
                if ((matches == null) || (matches.IsEmpty())) {
                    continue;
                }

                // evaluate expression
                IEnumerator<EventBean> eventsIt = matches.GetEnumerator();
                while (eventsIt.MoveNext()) {
                    eventsPerStream[0] = eventsIt.Current;

                    foreach (EventBean aNewData in newData) {
                        eventsPerStream[1] = aNewData; // Stream 1 events are the originating events (on-delete events)

                        Boolean result = (Boolean) joinExpr.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                        if (result != null) {
                            if (result) {
                                if (foundEvents == null) {
                                    foundEvents = new LinkedHashSet<EventBean>();
                                }

                                foundEvents.Add(eventsPerStream[0]);
                            }
                        }
                    }
                }
            }

            if (foundEvents == null) {
                exprEvaluatorContext.InstrumentationProvider.AInfraTriggeredLookup(null);
                return null;
            }

            EventBean[] events = foundEvents.ToArray();
            exprEvaluatorContext.InstrumentationProvider.AInfraTriggeredLookup(events);
            return events;
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }

        public string ToQueryPlan()
        {
            return this.GetType().Name + " " + " strategy " + tableLookupStrategy.ToQueryPlan();
        }
    }
} // end of namespace