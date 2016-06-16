///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.rep;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class FullTableScanUniqueValueLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly EventTableAsSet _eventIndex;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventIndex">table to use</param>
        public FullTableScanUniqueValueLookupStrategy(EventTableAsSet eventIndex)
        {
            _eventIndex = eventIndex;
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QIndexJoinLookup(this, _eventIndex); }
            ISet<EventBean> result = _eventIndex.AllValues;
            if (result.IsEmpty())
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexJoinLookup(null, null); }
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AIndexJoinLookup(result, null); }
            return result;
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN, null); }
        }
    }
}
