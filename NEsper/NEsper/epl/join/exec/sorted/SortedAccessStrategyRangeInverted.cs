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
    public class SortedAccessStrategyRangeInverted : SortedAccessStrategyRangeBase, SortedAccessStrategy
    {
        public SortedAccessStrategyRangeInverted(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator start, bool includeStart, ExprEvaluator end, bool includeEnd)
                    : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd)
        {
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context)
        {
            return index.LookupRangeInverted(base.EvaluateLookupStart(theEvent, context), IncludeStart, base.EvaluateLookupEnd(theEvent, context), IncludeEnd);
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object start = base.EvaluateLookupStart(theEvent, context);
            keys.Add(start);
            Object end = base.EvaluateLookupEnd(theEvent, context);
            keys.Add(end);
            return index.LookupRangeInverted(start, IncludeStart, end, IncludeEnd);
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context)
        {
            return index.LookupRangeInvertedColl(base.EvaluatePerStreamStart(eventsPerStream, context), IncludeStart, base.EvaluatePerStreamEnd(eventsPerStream, context), IncludeEnd);
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object start = base.EvaluatePerStreamStart(eventsPerStream, context);
            keys.Add(start);
            Object end = base.EvaluatePerStreamEnd(eventsPerStream, context);
            keys.Add(end);
            return index.LookupRangeInvertedColl(start, IncludeStart, end, IncludeEnd);
        }
    }
    
}
