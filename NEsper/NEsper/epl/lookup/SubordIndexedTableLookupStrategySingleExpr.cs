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
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategySingleExpr : SubordTableLookupStrategy
    {
        /// <summary>Stream numbers to get key values from. </summary>
        protected readonly ExprEvaluator Evaluator;
    
        private readonly EventBean[] _events;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        /// <summary>MapIndex to look up in. </summary>
        protected readonly PropertyIndexedEventTableSingle Index;
    
        public SubordIndexedTableLookupStrategySingleExpr(int streamCountOuter, ExprEvaluator evaluator, PropertyIndexedEventTableSingle index, LookupStrategyDesc strategyDesc)
        {
            Evaluator = evaluator;
            Index = index;
            _events = new EventBean[streamCountOuter+1];
            _strategyDesc = strategyDesc;
        }
    
        /// <summary>Returns index to look up in. </summary>
        /// <returns>index to use</returns>
        public PropertyIndexedEventTableSingle GetIndex()
        {
            return Index;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, Index, null); }
    
            Object key = GetKey(eventsPerStream, context);
    
            if (InstrumentationHelper.ENABLED) {
                ISet<EventBean> result = Index.Lookup(key);
                InstrumentationHelper.Get().AIndexSubordLookup(result, key);
                return result;
            }
            return Index.Lookup(key);
        }

        /// <summary>
        /// Get the index lookup keys.
        /// </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <param name="context">The context.</param>
        /// <returns>key object</returns>
        protected virtual Object GetKey(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return Evaluator.Evaluate(new EvaluateParams(_events, true, context));
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        public String ToQueryPlan()
        {
            return GetType().FullName + " evaluator " + Evaluator.GetType().Name;
        }
    }
}
