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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class SubordFullTableScanLookupStrategy : SubordTableLookupStrategy
    {
        private readonly UnindexedEventTable _eventIndex;
    
        /// <summary>Ctor. </summary>
        /// <param name="eventIndex">table to use</param>
        public SubordFullTableScanLookupStrategy(UnindexedEventTable eventIndex)
        {
            _eventIndex = eventIndex;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventPerStream, ExprEvaluatorContext context)
        {
            return LookupInternal();
        }
    
        private ICollection<EventBean> LookupInternal()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexSubordLookup(this, _eventIndex, null);}
            ICollection<EventBean> result = _eventIndex.EventSet;
            if (result.IsEmpty())
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexSubordLookup(null, null);}
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexSubordLookup(result, null);}
            return result;
        }
    
        public String ToQueryPlan() {
            return this.GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN, null);
    }
}
