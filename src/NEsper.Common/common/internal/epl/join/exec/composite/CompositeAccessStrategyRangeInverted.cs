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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeAccessStrategyRangeInverted : CompositeAccessStrategyRangeBase,
        CompositeAccessStrategy
    {
        public CompositeAccessStrategyRangeInverted(
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

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            ICollection<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = EvaluateLookupStart(theEvent, context);
            optionalKeyCollector?.Add(comparableStart);

            if (comparableStart == null) {
                return null;
            }

            var comparableEnd = EvaluateLookupEnd(theEvent, context);
            optionalKeyCollector?.Add(comparableEnd);

            if (comparableEnd == null) {
                return null;
            }

            var index = (IOrderedDictionary<object, CompositeIndexEntry>)parent;
            var submapOne = index.Head(comparableStart, !includeStart);
            var submapTwo = index.Tail(comparableEnd, !includeEnd);
            return CompositeIndexQueryRange.Handle(
                theEvent,
                submapOne,
                submapTwo,
                result,
                next,
                postProcessor);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            ICollection<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = EvaluatePerStreamStart(eventsPerStream, context);
            optionalKeyCollector?.Add(comparableStart);

            if (comparableStart == null) {
                return null;
            }

            var comparableEnd = EvaluatePerStreamEnd(eventsPerStream, context);
            optionalKeyCollector?.Add(comparableEnd);

            if (comparableEnd == null) {
                return null;
            }

            var index = (IOrderedDictionary<object, CompositeIndexEntry>)parent;
            var submapOne = index.Head(comparableStart, !includeStart);
            var submapTwo = index.Tail(comparableEnd, !includeEnd);
            return CompositeIndexQueryRange.Handle(eventsPerStream, submapOne, submapTwo, result, next, postProcessor);
        }
    }
} // end of namespace