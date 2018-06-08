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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>Index lookup strategy for subqueries.</summary>
    public class SubordInKeywordMultiTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly ExprEvaluator _evaluator;
        private readonly EventBean[] _events;

        /// <summary>Index to look up in.</summary>
        private readonly PropertyIndexedEventTableSingle[] _indexes;

        private readonly LookupStrategyDesc strategyDesc;

        public SubordInKeywordMultiTableLookupStrategy(
            int numStreamsOuter,
            ExprEvaluator evaluator,
            EventTable[] tables,
            LookupStrategyDesc strategyDesc)
        {
            _evaluator = evaluator;
            this.strategyDesc = strategyDesc;
            _events = new EventBean[numStreamsOuter + 1];
            _indexes = new PropertyIndexedEventTableSingle[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                _indexes[i] = (PropertyIndexedEventTableSingle) tables[i];
            }
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return InKeywordTableLookupUtil.MultiIndexLookup(_evaluator, _events, context, _indexes);
        }

        public LookupStrategyDesc StrategyDesc => strategyDesc;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace