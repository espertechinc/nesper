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
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategy : SubordTableLookupStrategy
    {
        /// <summary>Index to look up in. </summary>
        protected readonly PropertyIndexedEventTableSingle[] Indexes;
    
        protected readonly ExprEvaluator Evaluator;
        private readonly LookupStrategyDesc _strategyDesc;
        private readonly EventBean[] _events;
    
        /// <summary>Ctor. </summary>
        public SubordInKeywordMultiTableLookupStrategy(int numStreamsOuter, ExprEvaluator evaluator, EventTable[] tables, LookupStrategyDesc strategyDesc)
        {
            Evaluator = evaluator;
            _strategyDesc = strategyDesc;
            _events = new EventBean[numStreamsOuter + 1];
            Indexes = new PropertyIndexedEventTableSingle[tables.Length];
            for (int i = 0; i < tables.Length; i++) {
                Indexes[i] = (PropertyIndexedEventTableSingle) tables[i];
            }
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return InKeywordTableLookupUtil.MultiIndexLookup(Evaluator, _events, context, Indexes);
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return _strategyDesc; }
        }

        public String ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
