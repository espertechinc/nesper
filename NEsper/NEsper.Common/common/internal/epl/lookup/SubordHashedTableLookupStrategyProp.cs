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
using com.espertech.esper.common.@internal.epl.index.hash;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordHashedTableLookupStrategyProp : SubordTableLookupStrategy
    {
        private readonly SubordHashedTableLookupStrategyPropFactory _factory;
        private readonly PropertyHashedEventTable _index;

        public SubordHashedTableLookupStrategyProp(
            SubordHashedTableLookupStrategyPropFactory factory, PropertyHashedEventTable index)
        {
            this._factory = factory;
            this._index = index;
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, _index, _factory.KeyStreamNums);
                var keyX = GetKey(eventsPerStream, context);
                ISet<EventBean> result = _index.Lookup(keyX);
                context.InstrumentationProvider.AIndexSubordLookup(result, keyX);
                return result;
            }

            var key = GetKey(eventsPerStream, context);
            return _index.Lookup(key);
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        public string ToQueryPlan()
        {
            return _factory.ToQueryPlan();
        }

        protected object GetKey(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return _factory.Evaluator.Evaluate(eventsPerStream, true, context);
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }
    }
} // end of namespace