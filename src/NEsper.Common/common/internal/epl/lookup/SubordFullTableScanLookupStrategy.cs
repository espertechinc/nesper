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
using com.espertech.esper.common.@internal.epl.index.unindexed;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class SubordFullTableScanLookupStrategy : SubordTableLookupStrategy
    {
        private readonly UnindexedEventTable _eventIndex;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventIndex">table to use</param>
        public SubordFullTableScanLookupStrategy(UnindexedEventTable eventIndex)
        {
            this._eventIndex = eventIndex;
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        public LookupStrategyDesc StrategyDesc => LookupStrategyDesc.SCAN;

        public ICollection<EventBean> Lookup(
            EventBean[] eventPerStream,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, _eventIndex, null);
                var result = LookupInternal();
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return LookupInternal();
        }

        private ISet<EventBean> LookupInternal()
        {
            ISet<EventBean> result = _eventIndex.EventSet;
            if (result.IsEmpty()) {
                return null;
            }

            return result;
        }
    }
} // end of namespace