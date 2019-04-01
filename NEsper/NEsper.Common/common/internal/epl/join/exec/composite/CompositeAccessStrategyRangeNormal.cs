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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeAccessStrategyRangeNormal : CompositeAccessStrategyRangeBase,
        CompositeAccessStrategy
    {
        private bool allowReverseRange;

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
            this.allowReverseRange = allowReverseRange;
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
            object comparableStart = base.EvaluateLookupStart(theEvent, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableStart);
            }

            if (comparableStart == null) {
                return null;
            }

            object comparableEnd = base.EvaluateLookupEnd(theEvent, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableEnd);
            }

            if (comparableEnd == null) {
                return null;
            }

            OrderedDictionary<object, CompositeIndexEntry> index =
                (OrderedDictionary<object, CompositeIndexEntry>) parent;

            IDictionary<object, CompositeIndexEntry> submap;
            try {
                submap = index.Between(comparableStart, includeStart, comparableEnd, includeEnd);
            }
            catch (ArgumentException ex) {
                if (allowReverseRange) {
                    submap = index.Between(comparableEnd, includeStart, comparableStart, includeEnd);
                }
                else {
                    return null;
                }
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
            object comparableStart = base.EvaluatePerStreamStart(eventPerStream, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableStart);
            }

            if (comparableStart == null) {
                return null;
            }

            object comparableEnd = base.EvaluatePerStreamEnd(eventPerStream, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableEnd);
            }

            if (comparableEnd == null) {
                return null;
            }

            var index = (OrderedDictionary<object, CompositeIndexEntry>) parent;
            IDictionary<object, CompositeIndexEntry> submap;
            try {
                submap = index.Between(comparableStart, includeStart, comparableEnd, includeEnd);
            }
            catch (ArgumentException) {
                if (allowReverseRange) {
                    submap = index.Between(comparableEnd, includeStart, comparableStart, includeEnd);
                }
                else {
                    return null;
                }
            }

            return CompositeIndexQueryRange.Handle(eventPerStream, submap, null, result, next, postProcessor);
        }
    }
} // end of namespace