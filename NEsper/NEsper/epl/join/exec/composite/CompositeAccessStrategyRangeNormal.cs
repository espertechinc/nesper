///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.exec.composite
{
    using TreeMap = OrderedDictionary<object, object>;
    using Map = IDictionary<object, object>;

    public class CompositeAccessStrategyRangeNormal
        : CompositeAccessStrategyRangeBase
        , CompositeAccessStrategy
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
            Type coercionType,
            bool allowReverseRange)
            : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd, coercionType)
        {
            _allowReverseRange = allowReverseRange;
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Map parent, ICollection<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector, CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = base.EvaluateLookupStart(theEvent, context);
            if (optionalKeyCollector != null)
            {
                optionalKeyCollector.Add(comparableStart);
            }
            if (comparableStart == null)
            {
                return null;
            }
            var comparableEnd = base.EvaluateLookupEnd(theEvent, context);
            if (optionalKeyCollector != null)
            {
                optionalKeyCollector.Add(comparableEnd);
            }
            if (comparableEnd == null)
            {
                return null;
            }
            var index = (OrderedDictionary<object, object>) parent;
            comparableStart = EventBeanUtility.Coerce(comparableStart, CoercionType);
            comparableEnd = EventBeanUtility.Coerce(comparableEnd, CoercionType);

            IDictionary<object, object> submap;
            try
            {
                submap = index.Between(comparableStart, IncludeStart, comparableEnd, IncludeEnd);
            }
            catch (ArgumentException)
            {
                if (_allowReverseRange)
                {
                    submap = index.Between(comparableEnd, IncludeStart, comparableStart, IncludeEnd);
                }
                else
                {
                    return null;
                }
            }

            return CompositeIndexQueryRange.Handle(theEvent, submap, null, result, next, postProcessor);
        }

        public ICollection<EventBean> Lookup(EventBean[] eventPerStream, Map parent, ICollection<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector, CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = base.EvaluatePerStreamStart(eventPerStream, context);
            if (optionalKeyCollector != null)
            {
                optionalKeyCollector.Add(comparableStart);
            }
            if (comparableStart == null)
            {
                return null;
            }
            var comparableEnd = base.EvaluatePerStreamEnd(eventPerStream, context);
            if (optionalKeyCollector != null)
            {
                optionalKeyCollector.Add(comparableEnd);
            }
            if (comparableEnd == null)
            {
                return null;
            }
            var index = (OrderedDictionary<object, object>) parent;
            comparableStart = EventBeanUtility.Coerce(comparableStart, CoercionType);
            comparableEnd = EventBeanUtility.Coerce(comparableEnd, CoercionType);

            IDictionary<object, object> submap;
            try
            {
                submap = index.Between(comparableStart, IncludeStart, comparableEnd, IncludeEnd);
            }
            catch (ArgumentException)
            {
                if (_allowReverseRange)
                {
                    submap = index.Between(comparableEnd, IncludeStart, comparableStart, IncludeEnd);
                }
                else
                {
                    return null;
                }
            }

            return CompositeIndexQueryRange.Handle(eventPerStream, submap, null, result, next, postProcessor);
        }
    }
}