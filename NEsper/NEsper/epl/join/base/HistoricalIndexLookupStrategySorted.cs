///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.exec.sorted;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// MapIndex lookup strategy into a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategySorted : HistoricalIndexLookupStrategy
    {
        private readonly SortedAccessStrategy _strategy;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">The lookup stream.</param>
        /// <param name="property">The property.</param>
        public HistoricalIndexLookupStrategySorted(int lookupStream, QueryGraphValueEntryRange property)
        {
            _strategy = SortedAccessStrategyFactory.Make(false, lookupStream, -1, property, null);
        }

        /// <summary>
        /// Lookups the specified lookup event.
        /// </summary>
        /// <param name="lookupEvent">The lookup event.</param>
        /// <param name="indexTable">The index table.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] indexTable, ExprEvaluatorContext context)
        {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertySortedEventTable)
            {
                var index = (PropertySortedEventTable)indexTable[0];
                ICollection<EventBean> events = _strategy.Lookup(lookupEvent, index, context);
                if (events != null)
                {
                    return events.GetEnumerator();
                }
                return null;
            }

            return indexTable[0].GetEnumerator();
        }
    
        public String ToQueryPlan() 
        {
            return GetType().Name + " strategy: " + _strategy.ToQueryPlan();
        }
    }
}
