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

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries for in-keyword single-index sided.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyNW : SubordTableLookupStrategy
    {
        private readonly ExprEvaluator _evaluator;

        /// <summary>Index to look up in.</summary>
        private readonly PropertyIndexedEventTableSingle[] _indexes;

        private readonly LookupStrategyDesc _strategyDesc;

        public SubordInKeywordMultiTableLookupStrategyNW(
            ExprEvaluator evaluator,
            EventTable[] tables,
            LookupStrategyDesc strategyDesc)
        {
            _evaluator = evaluator;
            _indexes = new PropertyIndexedEventTableSingle[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                _indexes[i] = (PropertyIndexedEventTableSingle) tables[i];
            }
            _strategyDesc = strategyDesc;
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return InKeywordTableLookupUtil.MultiIndexLookup(_evaluator, eventsPerStream, context, _indexes);
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace