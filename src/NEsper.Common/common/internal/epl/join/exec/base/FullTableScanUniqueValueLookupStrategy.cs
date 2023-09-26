///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    /// Lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class FullTableScanUniqueValueLookupStrategy : JoinExecTableLookupStrategy
    {
        private EventTableAsSet eventIndex;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventIndex">table to use</param>
        public FullTableScanUniqueValueLookupStrategy(EventTableAsSet eventIndex)
        {
            this.eventIndex = eventIndex;
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QIndexJoinLookup(this, eventIndex);

            var result = eventIndex.AllValues();
            if (result.IsEmpty()) {
                instrumentationCommon.AIndexJoinLookup(null, null);
                return null;
            }

            instrumentationCommon.AIndexJoinLookup(result, null);
            return result;
        }

        public LookupStrategyDesc StrategyDesc => LookupStrategyDesc.SCAN;

        public LookupStrategyType LookupStrategyType => LookupStrategyType.FULLTABLESCAN;
    }
} // end of namespace