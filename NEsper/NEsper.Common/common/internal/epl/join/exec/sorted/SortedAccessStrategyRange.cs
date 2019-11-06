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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    public class SortedAccessStrategyRange : SortedAccessStrategyRangeBase,
        SortedAccessStrategy
    {
        // indicate whether "a between 60 and 50" should return no results (false, equivalent to a>= X and a <=Y) or should return results (true, equivalent to 'between' and 'in')
        private readonly bool _allowRangeReversal;

        public SortedAccessStrategyRange(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator start,
            bool includeStart,
            ExprEvaluator end,
            bool includeEnd,
            bool allowRangeReversal)
            : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd)
        {
            this._allowRangeReversal = allowRangeReversal;
        }

        public ISet<EventBean> Lookup(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupRange(
                base.EvaluateLookupStart(theEvent, context),
                includeStart,
                base.EvaluateLookupEnd(theEvent, context),
                includeEnd,
                _allowRangeReversal);
        }

        public ISet<EventBean> LookupCollectKeys(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            object start = base.EvaluateLookupStart(theEvent, context);
            keys.Add(start);
            object end = base.EvaluateLookupEnd(theEvent, context);
            keys.Add(end);
            return index.LookupRange(start, includeStart, end, includeEnd, _allowRangeReversal);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupRangeColl(
                base.EvaluatePerStreamStart(eventsPerStream, context),
                includeStart,
                base.EvaluatePerStreamEnd(eventsPerStream, context),
                includeEnd,
                _allowRangeReversal);
        }

        public ICollection<EventBean> LookupCollectKeys(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            object start = base.EvaluatePerStreamStart(eventsPerStream, context);
            keys.Add(start);
            object end = base.EvaluatePerStreamEnd(eventsPerStream, context);
            keys.Add(end);
            return index.LookupRangeColl(start, includeStart, end, includeEnd, _allowRangeReversal);
        }
    }
} // end of namespace