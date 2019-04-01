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
    public class SubordHashedTableLookupStrategyExpr : SubordTableLookupStrategy
    {
        private readonly EventBean[] _events;
        private readonly SubordHashedTableLookupStrategyExprFactory _factory;

        public SubordHashedTableLookupStrategyExpr(
            SubordHashedTableLookupStrategyExprFactory factory, PropertyHashedEventTable index)
        {
            this._factory = factory;
            _events = new EventBean[factory.NumStreamsOuter + 1];
            Index = index;
        }

        /// <summary>
        ///     Returns index to look up in.
        /// </summary>
        /// <returns>index to use</returns>
        public PropertyHashedEventTable Index { get; }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, Index, null);
                var key = GetKey(eventsPerStream, context);
                ISet<EventBean> result = Index.Lookup(key);
                context.InstrumentationProvider.AIndexSubordLookup(result, key);
                return result;
            }

            var key = GetKey(eventsPerStream, context);
            return Index.Lookup(key);
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        public string ToQueryPlan()
        {
            return _factory.ToQueryPlan();
        }

        /// <summary>
        ///     Get the index lookup keys.
        /// </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <param name="context">context</param>
        /// <returns>key object</returns>
        protected object GetKey(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return _factory.Evaluator.Evaluate(_events, true, context);
        }
    }
} // end of namespace