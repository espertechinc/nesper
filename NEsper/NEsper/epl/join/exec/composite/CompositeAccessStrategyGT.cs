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
    using DataMap = IDictionary<object, object>;

    public class CompositeAccessStrategyGT
        : CompositeAccessStrategyRelOpBase
        , CompositeAccessStrategy
    {
        public CompositeAccessStrategyGT(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator key,
            Type coercionType)
            : base(isNWOnTrigger, lookupStream, numStreams, key, coercionType)
        {
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            DataMap parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            IList<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var index = (OrderedDictionary<object, object>)parent;
            var comparable = EvaluateLookup(theEvent, context);
            if (optionalKeyCollector != null)
                optionalKeyCollector.Add(comparable);
            if (comparable == null)
                return null;
            comparable = EventBeanUtility.Coerce(comparable, CoercionType);
            return CompositeIndexQueryRange.Handle(theEvent, index.Tail(comparable, false), null, result, next, postProcessor);
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventPerStream,
            DataMap parent,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            ExprEvaluatorContext context,
            IList<object> optionalKeyCollector,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var index = (OrderedDictionary<object, object>) parent;
            var comparable = EvaluatePerStream(eventPerStream, context);
            if (optionalKeyCollector != null)
                optionalKeyCollector.Add(comparable);
            if (comparable == null)
                return null;
            comparable = EventBeanUtility.Coerce(comparable, CoercionType);
            return CompositeIndexQueryRange.Handle(
                eventPerStream, index.Tail(comparable, false), null, result, next, postProcessor);
        }
    }
}
