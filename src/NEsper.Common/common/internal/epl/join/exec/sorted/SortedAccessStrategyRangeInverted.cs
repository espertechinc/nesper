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
                base.EvaluateLookupStart(theEvent, context),
                includeStart,
                base.EvaluateLookupEnd(theEvent, context),
                includeEnd);
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
            return index.LookupRangeInverted(start, includeStart, end, includeEnd);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupRangeInvertedColl(
                base.EvaluatePerStreamStart(eventsPerStream, context),
                includeStart,
                base.EvaluatePerStreamEnd(eventsPerStream, context),
                includeEnd);
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
            return index.LookupRangeInvertedColl(start, includeStart, end, includeEnd);
        }
    }
} // end of namespace