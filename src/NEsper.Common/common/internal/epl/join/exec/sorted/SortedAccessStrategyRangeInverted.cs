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
using com.espertech.esper.common.@internal.epl.index.sorted;

namespace com.espertech.esper.common.@internal.epl.join.exec.sorted
{
    public class SortedAccessStrategyRangeInverted : SortedAccessStrategyRangeBase,
        SortedAccessStrategy
    {
        public SortedAccessStrategyRangeInverted(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator start,
            bool includeStart,
            ExprEvaluator end,
            bool includeEnd)
            : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd)
        {
        }

        public ISet<EventBean> Lookup(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupRangeInverted(
                EvaluateLookupStart(theEvent, context),
                includeStart,
                EvaluateLookupEnd(theEvent, context),
                includeEnd);
        }

        public ISet<EventBean> LookupCollectKeys(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            var start = EvaluateLookupStart(theEvent, context);
            keys.Add(start);
            var end = EvaluateLookupEnd(theEvent, context);
            keys.Add(end);
            return index.LookupRangeInverted(start, includeStart, end, includeEnd);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupRangeInvertedColl(
                EvaluatePerStreamStart(eventsPerStream, context),
                includeStart,
                EvaluatePerStreamEnd(eventsPerStream, context),
                includeEnd);
        }

        public ICollection<EventBean> LookupCollectKeys(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            var start = EvaluatePerStreamStart(eventsPerStream, context);
            keys.Add(start);
            var end = EvaluatePerStreamEnd(eventsPerStream, context);
            keys.Add(end);
            return index.LookupRangeInvertedColl(start, includeStart, end, includeEnd);
        }
    }
} // end of namespace