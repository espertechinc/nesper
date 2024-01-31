///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.join.exec.sorted;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategySorted : HistoricalIndexLookupStrategy
    {
        private QueryGraphValueEntryRange evalRange;
        private int lookupStream;
        private SortedAccessStrategy strategy;

        public int LookupStream {
            set => lookupStream = value;
        }

        public QueryGraphValueEntryRange EvalRange {
            set => evalRange = value;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context)
        {
            if (index[0] is PropertySortedEventTable) {
                var idx = (PropertySortedEventTable)index[0];
                var events = strategy.Lookup(lookupEvent, idx, context);
                return events?.GetEnumerator();
            }

            return index[0].GetEnumerator();
        }

        public void Init()
        {
            strategy = SortedAccessStrategyFactory.Make(false, lookupStream, -1, evalRange);
        }
    }
} // end of namespace