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
using com.espertech.esper.common.@internal.epl.index.composite;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordCompositeTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly SubordCompositeTableLookupStrategyFactory _factory;
        private readonly PropertyCompositeEventTable _index;

        public SubordCompositeTableLookupStrategy(
            SubordCompositeTableLookupStrategyFactory factory, PropertyCompositeEventTable index)
        {
            this._factory = factory;
            this._index = index;
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, _index, null);
                var keys = new List<object>(2); // can collect nulls
                ICollection<EventBean> result = _factory.InnerIndexQuery.GetCollectKeys(
                    eventsPerStream, _index.Index, context, keys, _index.PostProcessor);
                context.InstrumentationProvider.AIndexSubordLookup(
                    result, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }

            return _factory.InnerIndexQuery.Get(eventsPerStream, _index.Index, context, _index.PostProcessor);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;
    }
} // end of namespace