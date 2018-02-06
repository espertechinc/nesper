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
using com.espertech.esper.epl.@join.exec.sorted;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordSortedTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly SortedAccessStrategy _strategy;
    
        /// <summary>MapIndex to look up in. </summary>
        private readonly PropertySortedEventTable _index;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordSortedTableLookupStrategy(SortedAccessStrategy strategy, PropertySortedEventTable index, LookupStrategyDesc strategyDesc)
        {
            _strategy = strategy;
            _index = index;
            _strategyDesc = strategyDesc;
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null);
                List<Object> keys = new List<Object>(2);
                ICollection<EventBean> result = _strategy.LookupCollectKeys(eventsPerStream, _index, context, keys);
                InstrumentationHelper.Get().AIndexSubordLookup(result, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }

            return _strategy.Lookup(eventsPerStream, _index, context);
        }
    
        public String ToQueryPlan()
        {
            return GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;
    }
}
