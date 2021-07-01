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
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.composite
{
    public class CompositeIndexQueryRange : CompositeIndexQuery
    {
        private readonly CompositeAccessStrategy _strategy;
        private CompositeIndexQuery _next;

        public CompositeIndexQueryRange(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            QueryGraphValueEntryRange rangeProp)
        {
            if (rangeProp.Type.IsRange()) {
                var rangeIn = (QueryGraphValueEntryRangeIn) rangeProp;
                var start = rangeIn.ExprStart;
                var includeStart = rangeProp.Type.IsIncludeStart();

                var end = rangeIn.ExprEnd;
                var includeEnd = rangeProp.Type.IsIncludeEnd();

                if (!rangeProp.Type.IsRangeInverted()) {
                    _strategy = new CompositeAccessStrategyRangeNormal(
                        isNWOnTrigger,
                        lookupStream,
                        numStreams,
                        start,
                        includeStart,
                        end,
                        includeEnd,
                        rangeIn.IsAllowRangeReversal);
                }
                else {
                    _strategy = new CompositeAccessStrategyRangeInverted(
                        isNWOnTrigger,
                        lookupStream,
                        numStreams,
                        start,
                        includeStart,
                        end,
                        includeEnd);
                }
            }
            else {
                var relOp = (QueryGraphValueEntryRangeRelOp) rangeProp;
                var key = relOp.Expression;
                if (rangeProp.Type == QueryGraphRangeEnum.GREATER_OR_EQUAL) {
                    _strategy = new CompositeAccessStrategyGE(isNWOnTrigger, lookupStream, numStreams, key);
                }
                else if (rangeProp.Type == QueryGraphRangeEnum.GREATER) {
                    _strategy = new CompositeAccessStrategyGT(isNWOnTrigger, lookupStream, numStreams, key);
                }
                else if (rangeProp.Type == QueryGraphRangeEnum.LESS_OR_EQUAL) {
                    _strategy = new CompositeAccessStrategyLE(isNWOnTrigger, lookupStream, numStreams, key);
                }
                else if (rangeProp.Type == QueryGraphRangeEnum.LESS) {
                    _strategy = new CompositeAccessStrategyLT(isNWOnTrigger, lookupStream, numStreams, key);
                }
                else {
                    throw new ArgumentException("Comparison operator " + rangeProp.Type + " not supported");
                }
            }
        }

        public void Add(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            _strategy.Lookup(eventsPerStream, parent, result, _next, null, null, postProcessor);
        }

        public void Add(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            _strategy.Lookup(theEvent, parent, result, _next, null, null, postProcessor);
        }

        public ICollection<EventBean> Get(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(theEvent, parent, null, _next, context, null, postProcessor);
        }

        public ICollection<EventBean> Get(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, null, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(theEvent, parent, null, _next, context, keys, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> parent,
            ExprEvaluatorContext context,
            ICollection<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, keys, postProcessor);
        }

        protected internal static ICollection<EventBean> Handle(
            EventBean theEvent,
            IDictionary<object, CompositeIndexEntry> sortedMapOne,
            IDictionary<object, CompositeIndexEntry> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (next == null) {
                if (result == null) {
                    result = new HashSet<EventBean>();
                }

                AddResults(sortedMapOne, sortedMapTwo, result, postProcessor);
                return result;
            }
            else {
                if (result == null) {
                    result = new HashSet<EventBean>();
                }

                var map = sortedMapOne;
                foreach (var entry in map) {
                    next.Add(theEvent, entry.Value.AssertIndex(), result, postProcessor);
                }

                if (sortedMapTwo != null) {
                    map = sortedMapTwo;
                    foreach (var entry in map) {
                        next.Add(theEvent, entry.Value.AssertIndex(), result, postProcessor);
                    }
                }

                return result;
            }
        }

        protected internal static ICollection<EventBean> Handle(
            EventBean[] eventsPerStream,
            IDictionary<object, CompositeIndexEntry> sortedMapOne,
            IDictionary<object, CompositeIndexEntry> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (next == null) {
                if (result == null) {
                    result = new HashSet<EventBean>();
                }

                AddResults(sortedMapOne, sortedMapTwo, result, postProcessor);
                return result;
            }
            else {
                if (result == null) {
                    result = new HashSet<EventBean>();
                }

                var map = sortedMapOne;
                foreach (var entry in map) {
                    next.Add(eventsPerStream, entry.Value.AssertIndex(), result, postProcessor);
                }

                if (sortedMapTwo != null) {
                    map = sortedMapTwo;
                    foreach (var entry in map) {
                        next.Add(eventsPerStream, entry.Value.AssertIndex(), result, postProcessor);
                    }
                }

                return result;
            }
        }

        private static void AddResults(
            IDictionary<object, CompositeIndexEntry> sortedMapOne,
            IDictionary<object, CompositeIndexEntry> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            AddResults(sortedMapOne, result, postProcessor);
            if (sortedMapTwo != null) {
                AddResults(sortedMapTwo, result, postProcessor);
            }
        }

        private static void AddResults(
            IDictionary<object, CompositeIndexEntry> sortedMapOne,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            var map = sortedMapOne;

            if (postProcessor != null) {
                foreach (var entry in map) {
                    postProcessor.Add(entry.Value, result);
                }
            }
            else {
                foreach (var entry in map) {
                    result.AddAll(entry.Value.AssertCollection());
                }
            }
        }

        public CompositeIndexQuery SetNext(CompositeIndexQuery next)
        {
            this._next = next;
            return this;
        }
    }
} // end of namespace