///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries for in-keyword single-index sided.
    /// </summary>
    public class SubordInKeywordSingleTableLookupStrategyNW : SubordTableLookupStrategy
    {
        /// <summary>Index to look up in.</summary>
        private readonly PropertyIndexedEventTableSingle _index;

        private readonly ExprEvaluator[] _evaluators;

        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordInKeywordSingleTableLookupStrategyNW(ExprEvaluator[] evaluators, PropertyIndexedEventTableSingle index, LookupStrategyDesc strategyDesc)
        {
            _evaluators = evaluators;
            _index = index;
            _strategyDesc = strategyDesc;
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTableSingle Index => _index;

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null);
            }
    
            var result = InKeywordTableLookupUtil.SingleIndexLookup(_evaluators, eventsPerStream, context, _index);
    
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AIndexSubordLookup(result, null);
            }
            return result;
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        public string ToQueryPlan() {
            return GetType().Name + " evaluators " + ExprNodeUtility.PrintEvaluators(_evaluators);
        }
    }
} // end of namespace
