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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.exec.sorted
{
    public class SortedAccessStrategyRange : SortedAccessStrategyRangeBase, SortedAccessStrategy
    {
        // indicate whether "a between 60 and 50" should return no results (false, equivalent to a>= X and a <=Y) or should return results (true, equivalent to 'between' and 'in')  
        private readonly bool allowRangeReversal;
    
        public SortedAccessStrategyRange(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator start, bool includeStart, ExprEvaluator end, bool includeEnd, bool allowRangeReversal)
                    : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd)
        {
            this.allowRangeReversal = allowRangeReversal;
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context)
        {
            return index.LookupRange(base.EvaluateLookupStart(theEvent, context), IncludeStart, base.EvaluateLookupEnd(theEvent, context), IncludeEnd, allowRangeReversal);
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object start = base.EvaluateLookupStart(theEvent, context);
            keys.Add(start);
            Object end = base.EvaluateLookupEnd(theEvent, context);
            keys.Add(end);
            return index.LookupRange(start, IncludeStart, end, IncludeEnd, allowRangeReversal);
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context)
        {
            return index.LookupRangeColl(base.EvaluatePerStreamStart(eventsPerStream, context), IncludeStart, base.EvaluatePerStreamEnd(eventsPerStream, context), IncludeEnd, allowRangeReversal);
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object start = base.EvaluatePerStreamStart(eventsPerStream, context);
            keys.Add(start);
            Object end = base.EvaluatePerStreamEnd(eventsPerStream, context);
            keys.Add(end);
            return index.LookupRangeColl(start, IncludeStart, end, IncludeEnd, allowRangeReversal);
        }
    }
}
