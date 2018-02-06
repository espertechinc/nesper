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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.composite;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordCompositeTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly CompositeIndexQuery _innerIndexQuery;
        private readonly PropertyCompositeEventTable _index;
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordCompositeTableLookupStrategy(CompositeIndexQuery innerIndexQuery, PropertyCompositeEventTable index, LookupStrategyDesc strategyDesc)
        {
            _innerIndexQuery = innerIndexQuery;
            _index = index;
            _strategyDesc = strategyDesc;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null);
                var keys = new List<Object>(); // can collect nulls
                var result = _innerIndexQuery.GetCollectKeys(eventsPerStream, _index.IndexTable, context, keys, _index.PostProcessor);
                InstrumentationHelper.Get().AIndexSubordLookup(result, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }
    
            return _innerIndexQuery.Get(eventsPerStream, _index.IndexTable, context, _index.PostProcessor);
        }
    
        public String ToQueryPlan() {
            return GetType().FullName;
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;
    }
}
