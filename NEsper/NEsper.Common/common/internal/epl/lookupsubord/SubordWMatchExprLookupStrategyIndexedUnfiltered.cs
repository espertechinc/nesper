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
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyIndexedUnfiltered : SubordWMatchExprLookupStrategy
    {
        private readonly EventBean[] eventsPerStream;
        private readonly SubordTableLookupStrategy tableLookupStrategy;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tableLookupStrategy">the strategy for looking up in an index the matching events using correlation</param>
        public SubordWMatchExprLookupStrategyIndexedUnfiltered(SubordTableLookupStrategy tableLookupStrategy)
        {
            eventsPerStream = new EventBean[2];
            this.tableLookupStrategy = tableLookupStrategy;
        }

        public EventBean[] Lookup(
            EventBean[] newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ISet<EventBean> removeEvents = null;

            // For every new event (usually 1)
            foreach (var newEvent in newData) {
                eventsPerStream[1] = newEvent;

                // use index to find match
                var matches = tableLookupStrategy.Lookup(eventsPerStream, exprEvaluatorContext);
                if (matches == null || matches.IsEmpty()) {
                    continue;
                }

                if (removeEvents == null) {
                    removeEvents = new LinkedHashSet<EventBean>();
                }

                removeEvents.AddAll(matches);
            }

            EventBean[] result = removeEvents?.ToArray();

            return result;
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " " + " strategy " + tableLookupStrategy.ToQueryPlan();
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }
    }
} // end of namespace