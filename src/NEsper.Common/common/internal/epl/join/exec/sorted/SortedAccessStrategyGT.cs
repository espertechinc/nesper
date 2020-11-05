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
    public class SortedAccessStrategyGT : SortedAccessStrategyRelOpBase,
        SortedAccessStrategy
    {
        public SortedAccessStrategyGT(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator keyEval)
            : base(isNWOnTrigger, lookupStream, numStreams, keyEval)
        {
        }

        public ISet<EventBean> Lookup(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupGreater(base.EvaluateLookup(theEvent, context));
        }

        public ISet<EventBean> LookupCollectKeys(
            EventBean theEvent,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            object point = base.EvaluateLookup(theEvent, context);
            keys.Add(point);
            return index.LookupGreater(point);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context)
        {
            return index.LookupGreaterColl(base.EvaluatePerStream(eventsPerStream, context));
        }

        public ICollection<EventBean> LookupCollectKeys(
            EventBean[] eventsPerStream,
            PropertySortedEventTable index,
            ExprEvaluatorContext context,
            List<object> keys)
        {
            object point = base.EvaluatePerStream(eventsPerStream, context);
            keys.Add(point);
            return index.LookupGreaterColl(point);
        }
    }
} // end of namespace