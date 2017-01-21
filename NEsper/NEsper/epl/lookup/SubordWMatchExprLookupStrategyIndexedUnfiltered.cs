///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    public class SubordWMatchExprLookupStrategyIndexedUnfiltered : SubordWMatchExprLookupStrategy
    {
        private readonly EventBean[] _eventsPerStream;
        private readonly SubordTableLookupStrategy _tableLookupStrategy;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableLookupStrategy">the strategy for looking up in an index the matching events using correlation</param>
        public SubordWMatchExprLookupStrategyIndexedUnfiltered(SubordTableLookupStrategy tableLookupStrategy)
        {
            _eventsPerStream = new EventBean[2];
            _tableLookupStrategy = tableLookupStrategy;
        }
    
        public EventBean[] Lookup(EventBean[] newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType.INDEXED_UNFILTERED); }
    
            ISet<EventBean> removeEvents = null;
    
            // For every new event (usually 1)
            foreach (EventBean newEvent in newData)
            {
                _eventsPerStream[1] = newEvent;
    
                // use index to find match
                ICollection<EventBean> matches = _tableLookupStrategy.Lookup(_eventsPerStream, exprEvaluatorContext);
                if ((matches == null) || (matches.IsEmpty()))
                {
                    continue;
                }
    
                if (removeEvents == null)
                {
                    removeEvents = new LinkedHashSet<EventBean>();
                }
                removeEvents.AddAll(matches);
            }
    
            if (removeEvents == null)
            {
                return null;
            }
    
            EventBean[] result = removeEvents.ToArray();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(result); }
    
            return result;
        }
    
        public override string ToString()
        {
            return ToQueryPlan();
        }
    
        public string ToQueryPlan()
        {
            return this.GetType().Name + " " + " strategy " + _tableLookupStrategy.ToQueryPlan();
        }
    }
}
