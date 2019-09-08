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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexQueryKeyed : CompositeIndexQuery
    {
        private readonly EventBean[] events;
        private readonly ExprEvaluator hashGetter;
        private readonly bool isNWOnTrigger;
        private readonly int lookupStream;

        private CompositeIndexQuery next;

        public CompositeIndexQueryKeyed(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            ExprEvaluator hashGetter)
        {
            this.hashGetter = hashGetter;
            this.isNWOnTrigger = isNWOnTrigger;
            this.lookupStream = lookupStream;

            if (lookupStream != -1) {
                events = new EventBean[lookupStream + 1];
            }
            else {
                events = new EventBean[numStreams + 1];
            }
        }

        public CompositeIndexQuery SetNext(CompositeIndexQuery next)
        {
            this.next = next;
            return this;
        }

        public ICollection<EventBean> Get(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            events[lookupStream] = theEvent;
            var mk = hashGetter.Evaluate(events, true, context);
            var innerEntry = parent.Get(mk);
            if (innerEntry == null) {
                return null;
            }

            var innerIndex = innerEntry.AssertIndex();
            return next.Get(theEvent, innerIndex, context, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            events[lookupStream] = theEvent;
            var mk = hashGetter.Evaluate(events, true, context);
            if (mk is MultiKey<object> multiKeyArray) {
                keys.AddAll(multiKeyArray.Array);
            }
            else {
                keys.Add(mk);
            }

            var innerEntry = parent.Get(mk);
            if (innerEntry == null) {
                return null;
            }

            var innerIndex = innerEntry.AssertIndex();
            return next.GetCollectKeys(theEvent, innerIndex, context, keys, postProcessor);
        }

        public ICollection<EventBean> Get(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            EventBean[] eventsToUse;
            if (isNWOnTrigger) {
                eventsToUse = eventsPerStream;
            }
            else {
                Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
                eventsToUse = events;
            }

            var mk = hashGetter.Evaluate(eventsToUse, true, context);
            var innerEntry = parent.Get(mk);
            if (innerEntry == null) {
                return null;
            }

            var innerIndex = innerEntry.AssertIndex();
            return next.Get(eventsPerStream, innerIndex, context, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            EventBean[] eventsToUse;
            if (isNWOnTrigger) {
                eventsToUse = eventsPerStream;
            }
            else {
                Array.Copy(eventsPerStream, 0, events, 1, eventsPerStream.Length);
                eventsToUse = events;
            }

            var mk = hashGetter.Evaluate(eventsToUse, true, context);
            if (mk is MultiKey<object> mkArray) {
                keys.AddAll(mkArray.Array);
            }
            else {
                keys.Add(mk);
            }

            var innerEntry = parent.Get(mk);
            if (innerEntry == null) {
                return null;
            }

            var innerIndex = innerEntry.AssertIndex();
            return next.GetCollectKeys(eventsPerStream, innerIndex, context, keys, postProcessor);
        }

        public void Add(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> value,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> value,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace