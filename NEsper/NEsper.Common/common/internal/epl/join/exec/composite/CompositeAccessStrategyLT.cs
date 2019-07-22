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
    public class CompositeAccessStrategyLT : CompositeAccessStrategyRelOpBase,
        CompositeAccessStrategy
    {
        public CompositeAccessStrategyLT(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator key)
            : base(isNWOnTrigger, lookupStream, numStreams, key)
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
            var index = (OrderedDictionary<object, CompositeIndexEntry>) parent;
            var comparable = EvaluateLookup(theEvent, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparable);
            }

            if (comparable == null) {
                return null;
            }

            return CompositeIndexQueryRange.Handle(
                theEvent,
                index.Head(comparable),
                null,
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
            var index = (OrderedDictionary<object, CompositeIndexEntry>) parent;
            var comparable = EvaluatePerStream(eventsPerStream, context);
            if (optionalKeyCollector != null) {
                optionalKeyCollector.Add(comparable);
            }

            if (comparable == null) {
                return null;
            }

            return CompositeIndexQueryRange.Handle(
                eventsPerStream,
                index.Head(comparable),
                null,
                result,
                next,
                postProcessor);
        }
    }
} // end of namespace