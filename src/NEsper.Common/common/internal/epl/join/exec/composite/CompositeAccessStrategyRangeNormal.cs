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
    public class CompositeAccessStrategyRangeNormal : CompositeAccessStrategyRangeBase,
        CompositeAccessStrategy
    {
        private readonly bool _allowReverseRange;

        public CompositeAccessStrategyRangeNormal(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator start,
            bool includeStart,
            ExprEvaluator end,
            bool includeEnd,
            bool allowReverseRange)
            : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd)
        {
            _allowReverseRange = allowReverseRange;
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
            object comparableStart = EvaluateLookupStart(theEvent, context);
            optionalKeyCollector?.Add(comparableStart);

            if (comparableStart == null) {
                return null;
            }

            object comparableEnd = EvaluateLookupEnd(theEvent, context);
            optionalKeyCollector?.Add(comparableEnd);

            if (comparableEnd == null) {
                return null;
            }

            IOrderedDictionary<object, CompositeIndexEntry> index =
                (IOrderedDictionary<object, CompositeIndexEntry>) parent;

            
            IDictionary<object, CompositeIndexEntry> submap;
            if (index.KeyComparer.Compare(comparableStart, comparableEnd) <= 0) {
                submap = index.Between(comparableStart, includeStart, comparableEnd, includeEnd);
            }
            else if (_allowReverseRange) {
                submap = index.Between(comparableEnd, includeStart, comparableStart, includeEnd);
            }
            else {
                return null;
            }

            return CompositeIndexQueryRange.Handle(theEvent, submap, null, result, next, postProcessor);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            ICollection<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            object comparableStart = EvaluatePerStreamStart(eventPerStream, context);
            optionalKeyCollector?.Add(comparableStart);

            if (comparableStart == null) {
                return null;
            }

            object comparableEnd = EvaluatePerStreamEnd(eventPerStream, context);
            optionalKeyCollector?.Add(comparableEnd);

            if (comparableEnd == null) {
                return null;
            }

            var index = (IOrderedDictionary<object, CompositeIndexEntry>) parent;
         
            IDictionary<object, CompositeIndexEntry> submap;

            if (index.KeyComparer.Compare(comparableStart, comparableEnd) <= 0) {
                submap = index.Between(comparableStart, includeStart, comparableEnd, includeEnd);
            }
            else if (_allowReverseRange) {
                submap = index.Between(comparableEnd, includeStart, comparableStart, includeEnd);
            }
            else {
                return null;
            }

            return CompositeIndexQueryRange.Handle(eventPerStream, submap, null, result, next, postProcessor);
        }
    }
} // end of namespace