///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.sorted;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordSortedTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly SubordSortedTableLookupStrategyFactory _factory;
        private readonly PropertySortedEventTable _index;

        public SubordSortedTableLookupStrategy(
            SubordSortedTableLookupStrategyFactory factory,
            PropertySortedEventTable index)
        {
            _factory = factory;
            _index = index;
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, _index, null);
                var keys = new List<object>(2);
                ICollection<EventBean> result = _factory.strategy.LookupCollectKeys(
                    eventsPerStream,
                    _index,
                    context,
                    keys);
                context.InstrumentationProvider.AIndexSubordLookup(
                    result,
                    keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }

            return _factory.strategy.Lookup(eventsPerStream, _index, context);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace