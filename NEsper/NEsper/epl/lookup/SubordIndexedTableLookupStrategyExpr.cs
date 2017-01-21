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
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategyExpr : SubordTableLookupStrategy
    {
        /// <summary>MapIndex to look up in. </summary>
        private readonly PropertyIndexedEventTable _index;

        private readonly ExprEvaluator[] _evaluators;
        private readonly LookupStrategyDesc _strategyDesc;
        private readonly EventBean[] _events;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="numStreamsOuter">The num streams outer.</param>
        /// <param name="evaluators">The evaluators.</param>
        /// <param name="index">is the table carrying the data to lookup into</param>
        /// <param name="strategyDesc">The strategy desc.</param>
        public SubordIndexedTableLookupStrategyExpr(int numStreamsOuter, ExprEvaluator[] evaluators, PropertyIndexedEventTable index, LookupStrategyDesc strategyDesc)
        {
            _evaluators = evaluators;
            _strategyDesc = strategyDesc;
            _events = new EventBean[numStreamsOuter + 1];
            _index = index;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTable Index
        {
            get { return _index; }
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null); }
    
            Object[] keys = GetKeys(eventsPerStream, context);
    
            if (InstrumentationHelper.ENABLED) {
                ICollection<EventBean> result = _index.Lookup(keys);
                InstrumentationHelper.Get().AIndexSubordLookup(result, keys);
                return result;
            }
            return _index.Lookup(keys);
        }

        /// <summary>
        /// Get the index lookup keys.
        /// </summary>
        /// <param name="eventsPerStream">is the events for each stream</param>
        /// <param name="context">The context.</param>
        /// <returns>key object</returns>
        protected virtual Object[] GetKeys(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            var keyValues = new Object[_evaluators.Length];
            var evaluateParams = new EvaluateParams(_events, true, context);
            for (int i = 0; i < _evaluators.Length; i++)
            {
                keyValues[i] = _evaluators[i].Evaluate(evaluateParams);
            }
            return keyValues;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _strategyDesc; }
        }

        public String ToQueryPlan() {
            return GetType().FullName + " evaluators " + ExprNodeUtility.PrintEvaluators(_evaluators);
        }
    }
}
