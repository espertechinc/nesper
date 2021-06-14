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
    public class CompositeAccessStrategyGE : CompositeAccessStrategyRelOpBase,
        CompositeAccessStrategy
    {
        public CompositeAccessStrategyGE(
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
            var index = (IOrderedDictionary<object, CompositeIndexEntry>) parent;
            var comparable = base.EvaluateLookup(theEvent, context);
            optionalKeyCollector?.Add(comparable);

            if (comparable == null) {
                return null;
            }

            return CompositeIndexQueryRange.Handle(
                theEvent,
                index.Tail(comparable),
                null,
                result,
                next,
                postProcessor);
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
            var index = (IOrderedDictionary<object, CompositeIndexEntry>) parent;
            var comparable = base.EvaluatePerStream(eventPerStream, context);
            optionalKeyCollector?.Add(comparable);

            if (comparable == null) {
                return null;
            }

            return CompositeIndexQueryRange.Handle(
                eventPerStream,
                index.Tail(comparable),
                null,
                result,
                next,
                postProcessor);
        }
    }
} // end of namespace