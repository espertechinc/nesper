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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.exec.composite
{
    using Map = IDictionary<object, object>;
    using TreeMap = OrderedDictionary<object, object>;

    public class CompositeAccessStrategyRangeInverted 
        : CompositeAccessStrategyRangeBase
        , CompositeAccessStrategy
    {
        public CompositeAccessStrategyRangeInverted(bool isNWOnTrigger, int lookupStream, int numStreams, ExprEvaluator start, bool includeStart, ExprEvaluator end, bool includeEnd, Type coercionType)
            : base(isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd, coercionType)
        {
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Map parent, ICollection<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector, CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = base.EvaluateLookupStart(theEvent, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableStart);
            }
            if (comparableStart == null) {
                return null;
            }
            var comparableEnd = base.EvaluateLookupEnd(theEvent, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableEnd);
            }
            if (comparableEnd == null) {
                return null;
            }
            comparableStart = EventBeanUtility.Coerce(comparableStart, CoercionType);
            comparableEnd = EventBeanUtility.Coerce(comparableEnd, CoercionType);
    
            var index = (TreeMap) parent;
            var submapOne = index.Head(comparableStart, !IncludeStart);
            var submapTwo = index.Tail(comparableEnd, !IncludeEnd);
            return CompositeIndexQueryRange.Handle(theEvent, submapOne, submapTwo, result, next, postProcessor);
        }

        public ICollection<EventBean> Lookup(EventBean[] eventPerStream, Map parent, ICollection<EventBean> result, CompositeIndexQuery next, ExprEvaluatorContext context, IList<object> optionalKeyCollector, CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var comparableStart = base.EvaluatePerStreamStart(eventPerStream, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableStart);
            }
            if (comparableStart == null) {
                return null;
            }
            var comparableEnd = base.EvaluatePerStreamEnd(eventPerStream, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparableEnd);
            }
            if (comparableEnd == null) {
                return null;
            }
            comparableStart = EventBeanUtility.Coerce(comparableStart, CoercionType);
            comparableEnd = EventBeanUtility.Coerce(comparableEnd, CoercionType);
    
            var index = (TreeMap) parent;
            var submapOne = index.Head(comparableStart, !IncludeStart);
            var submapTwo = index.Tail(comparableEnd, !IncludeEnd);
            return CompositeIndexQueryRange.Handle(eventPerStream, submapOne, submapTwo, result, next, postProcessor);
        }
    }
}
