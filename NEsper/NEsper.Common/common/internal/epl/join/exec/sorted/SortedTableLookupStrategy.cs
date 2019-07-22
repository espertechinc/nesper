///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    /// <summary>
    /// Lookup on an index that is a sorted index on a single property queried as a range.
    /// <para />Use the composite strategy if supporting multiple ranges or if range is in combination with unique key.
    /// </summary>
    public class SortedTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly PropertySortedEventTable index;
        private readonly SortedAccessStrategy strategy;

        public SortedTableLookupStrategy(
            int lookupStream,
            int numStreams,
            QueryGraphValueEntryRange rangeKeyPair,
            PropertySortedEventTable index)
        {
            this.index = index;
            this.strategy = SortedAccessStrategyFactory.Make(false, lookupStream, numStreams, rangeKeyPair);
        }

        /// <summary>
        /// Returns index to look up in.
        /// </summary>
        /// <returns>index to use</returns>
        public PropertySortedEventTable Index {
            get => index;
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            if (instrumentationCommon.Activated()) {
                instrumentationCommon.QIndexJoinLookup(this, index);
                List<object> keys = new List<object>(2);
                ISet<EventBean> result = strategy.LookupCollectKeys(theEvent, index, exprEvaluatorContext, keys);
                instrumentationCommon.AIndexJoinLookup(result, keys.Count > 1 ? keys.ToArray() : keys[0]);
                return result;
            }

            return strategy.Lookup(theEvent, index, exprEvaluatorContext);
        }

        public LookupStrategyType LookupStrategyType {
            get => LookupStrategyType.RANGE;
        }
    }
} // end of namespace