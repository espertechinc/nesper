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
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordSingleTableLookupStrategy : SubordTableLookupStrategy
    {
        /// <summary>Stream numbers to get key values from. </summary>
        protected readonly ExprEvaluator[] Evaluators;
    
        private readonly EventBean[] _events;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        /// <summary>Index to look up in. </summary>
        private PropertyIndexedEventTableSingle _index;
    
        public SubordInKeywordSingleTableLookupStrategy(int streamCountOuter, ExprEvaluator[] evaluators, PropertyIndexedEventTableSingle index, LookupStrategyDesc strategyDesc)
        {
            Evaluators = evaluators;
            _index = index;
            _events = new EventBean[streamCountOuter+1];
            _strategyDesc = strategyDesc;
        }

        /// <summary>Returns index to look up in. </summary>
        /// <value>index to use</value>
        public PropertyIndexedEventTableSingle Index
        {
            get => _index;
            internal set { _index = value; }
        }

        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _index, null);}
    
            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            ICollection<EventBean> result = InKeywordTableLookupUtil.SingleIndexLookup(Evaluators, _events, context, _index);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexSubordLookup(result, null);}
            return result;
        }

        public LookupStrategyDesc StrategyDesc => _strategyDesc;

        public String ToQueryPlan()
        {
            return this.GetType().Name;
        }
    }
}
