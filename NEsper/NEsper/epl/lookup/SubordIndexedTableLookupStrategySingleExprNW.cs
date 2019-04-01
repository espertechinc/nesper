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
    public class SubordIndexedTableLookupStrategySingleExprNW : SubordTableLookupStrategy
    {
        /// <summary>Stream numbers to get key values from. </summary>
        protected readonly ExprEvaluator Evaluator;
    
        /// <summary>MapIndex to look up in. </summary>
        private readonly PropertyIndexedEventTableSingle _index;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordIndexedTableLookupStrategySingleExprNW(ExprEvaluator evaluator, PropertyIndexedEventTableSingle index, LookupStrategyDesc strategyDesc)
        {
            Evaluator = evaluator;
            _index = index;
            _strategyDesc = strategyDesc;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public virtual PropertyIndexedEventTableSingle Index => _index;

        public virtual ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null); }
    
            Object key = GetKey(eventsPerStream, context);
    
            if (InstrumentationHelper.ENABLED) {
                ISet<EventBean> result = _index.Lookup(key);
                InstrumentationHelper.Get().AIndexSubordLookup(result, key);
                return result;
            }
    
            return _index.Lookup(key);
        }
    
        /// <summary>
        /// Get the index lookup keys.
        /// </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <param name="context">The context.</param>
        /// <returns>key object</returns>
        protected virtual Object GetKey(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return Evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, context));
        }

        public virtual LookupStrategyDesc StrategyDesc => _strategyDesc;

        public virtual String ToQueryPlan()
        {
            return GetType().FullName + " evaluator " + Evaluator.GetType().Name;
        }
    }
}
