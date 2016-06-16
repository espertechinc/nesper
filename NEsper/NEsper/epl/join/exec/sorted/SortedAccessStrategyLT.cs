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
    public class SortedAccessStrategyLT : SortedAccessStrategyRelOpBase, SortedAccessStrategy
    {
        public SortedAccessStrategyLT(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator keyEval)
                    : base(isNWOnTrigger, lookupStream, numStreams, keyEval)
        {
        }
    
        public ICollection<EventBean> Lookup(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context) {
            return index.LookupLess(base.EvaluateLookup(theEvent, context));
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean theEvent, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object point = base.EvaluateLookup(theEvent, context);
            keys.Add(point);
            return index.LookupLess(point);
        }
    
        public ICollection<EventBean> Lookup(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context)
        {
            return index.LookupLessThenColl(base.EvaluatePerStream(eventsPerStream, context));
        }
    
        public ICollection<EventBean> LookupCollectKeys(EventBean[] eventsPerStream, PropertySortedEventTable index, ExprEvaluatorContext context, IList<object> keys)
        {
            Object point = base.EvaluatePerStream(eventsPerStream, context);
            keys.Add(point);
            return index.LookupLessThenColl(point);
        }
    }
}
