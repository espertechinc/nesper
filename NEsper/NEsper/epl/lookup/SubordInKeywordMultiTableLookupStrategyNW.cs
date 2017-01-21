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
    /// <summary>
    /// Index lookup strategy for subqueries for in-keyword single-index sided.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyNW : SubordTableLookupStrategy
    {
        /// <summary>Index to look up in. </summary>
        protected readonly PropertyIndexedEventTableSingle[] Indexes;
    
        protected readonly ExprEvaluator Evaluator;
    
        public LookupStrategyDesc StrategyDesc { get; protected internal set; }
    
        /// <summary>Ctor. </summary>
        public SubordInKeywordMultiTableLookupStrategyNW(ExprEvaluator evaluator, EventTable[] tables, LookupStrategyDesc strategyDesc)
        {
            Evaluator = evaluator;
            Indexes = new PropertyIndexedEventTableSingle[tables.Length];
            for (int i = 0; i < tables.Length; i++) {
                Indexes[i] = (PropertyIndexedEventTableSingle) tables[i];
            }
            StrategyDesc = strategyDesc;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            return InKeywordTableLookupUtil.MultiIndexLookup(Evaluator, eventsPerStream, context, Indexes);
        }
    
        public String ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
