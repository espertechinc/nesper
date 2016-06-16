///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.exec.composite
{
    public class CompositeIndexQueryRange : CompositeIndexQuery
    {
        private readonly CompositeAccessStrategy _strategy;
        private CompositeIndexQuery _next;

        public CompositeIndexQueryRange(
            bool isNWOnTrigger,
            int lookupStream,
            int numStreams,
            SubordPropRangeKey subqRangeKey,
            Type coercionType,
            IList<string> expressionTexts)
        {
            var rangeProp = subqRangeKey.RangeInfo;

            if (rangeProp.RangeType.IsRange())
            {
                var rangeIn = (QueryGraphValueEntryRangeIn) rangeProp;
                var start = rangeIn.ExprStart.ExprEvaluator;
                expressionTexts.Add(rangeIn.ExprStart.ToExpressionStringMinPrecedenceSafe());
                var includeStart = rangeProp.RangeType.IsIncludeStart();

                var end = rangeIn.ExprEnd.ExprEvaluator;
                expressionTexts.Add(rangeIn.ExprEnd.ToExpressionStringMinPrecedenceSafe());
                var includeEnd = rangeProp.RangeType.IsIncludeEnd();

                if (!rangeProp.RangeType.IsRangeInverted())
                {
                    _strategy = new CompositeAccessStrategyRangeNormal(
                        isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd, coercionType,
                        ((QueryGraphValueEntryRangeIn) rangeProp).IsAllowRangeReversal);
                }
                else
                {
                    _strategy = new CompositeAccessStrategyRangeInverted(
                        isNWOnTrigger, lookupStream, numStreams, start, includeStart, end, includeEnd, coercionType);
                }
            }
            else
            {
                var relOp = (QueryGraphValueEntryRangeRelOp) rangeProp;
                var key = relOp.Expression.ExprEvaluator;
                expressionTexts.Add(relOp.Expression.ToExpressionStringMinPrecedenceSafe());
                switch (rangeProp.RangeType)
                {
                    case QueryGraphRangeEnum.GREATER_OR_EQUAL:
                        _strategy = new CompositeAccessStrategyGE(
                            isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                        break;
                    case QueryGraphRangeEnum.GREATER:
                        _strategy = new CompositeAccessStrategyGT(
                            isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                        break;
                    case QueryGraphRangeEnum.LESS_OR_EQUAL:
                        _strategy = new CompositeAccessStrategyLE(
                            isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                        break;
                    case QueryGraphRangeEnum.LESS:
                        _strategy = new CompositeAccessStrategyLT(
                            isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                        break;
                    default:
                        throw new ArgumentException("Comparison operator " + rangeProp.RangeType + " not supported");
                }
            }
        }

        public void Add(
            EventBean theEvent,
            IDictionary<object, object> parent,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            _strategy.Lookup(theEvent, parent, result, _next, null, null, postProcessor);
        }

        public void Add(
            EventBean[] eventsPerStream,
            IDictionary<object, object> parent,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            _strategy.Lookup(eventsPerStream, parent, result, _next, null, null, postProcessor);
        }

        public ICollection<EventBean> Get(
            EventBean theEvent,
            IDictionary<object, object> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(theEvent, parent, null, _next, context, null, postProcessor);
        }

        public ICollection<EventBean> Get(
            EventBean[] eventsPerStream,
            IDictionary<object, object> parent,
            ExprEvaluatorContext context,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, null, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean theEvent,
            IDictionary<object, object> parent,
            ExprEvaluatorContext context,
            IList<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(theEvent, parent, null, _next, context, keys, postProcessor);
        }

        public ICollection<EventBean> GetCollectKeys(
            EventBean[] eventsPerStream,
            IDictionary<object, object> parent,
            ExprEvaluatorContext context,
            IList<object> keys,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, keys, postProcessor);
        }

        protected internal static ICollection<EventBean> Handle(
            EventBean theEvent,
            IDictionary<object, object> sortedMapOne,
            IDictionary<object, object> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (next == null)
            {
                if (result == null)
                {
                    result = new HashSet<EventBean>();
                }
                AddResults(
                    sortedMapOne != null ?
                    sortedMapOne.Select(entry => new KeyValuePair<object, ICollection<EventBean>>(entry.Key, entry.Value as ICollection<EventBean>)) :
                    null,
                    sortedMapTwo != null ?
                    sortedMapTwo.Select(entry => new KeyValuePair<object, ICollection<EventBean>>(entry.Key, entry.Value as ICollection<EventBean>)) :
                    null,
                    result, 
                    postProcessor);
                return result;
            }
            else
            {
                if (result == null)
                {
                    result = new HashSet<EventBean>();
                }
                foreach (var entry in sortedMapOne)
                {
                    next.Add(theEvent, entry.Value as IDictionary<object, object>, result, postProcessor);
                }
                if (sortedMapTwo != null)
                {
                    foreach (var entry in sortedMapTwo)
                    {
                        next.Add(theEvent, entry.Value as IDictionary<object, object>, result, postProcessor);
                    }
                }
                return result;
            }
        }

        protected internal static ICollection<EventBean> Handle(
            EventBean[] eventsPerStream,
            IDictionary<object, object> sortedMapOne,
            IDictionary<object, object> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQuery next,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (next == null)
            {
                if (result == null)
                {
                    result = new HashSet<EventBean>();
                }
                AddResults(
                    sortedMapOne != null ?
                    sortedMapOne.Select(entry => new KeyValuePair<object, ICollection<EventBean>>(entry.Key, entry.Value as ICollection<EventBean>)) :
                    null,
                    sortedMapTwo != null ?
                    sortedMapTwo.Select(entry => new KeyValuePair<object, ICollection<EventBean>>(entry.Key, entry.Value as ICollection<EventBean>)) :
                    null,
                    result,
                    postProcessor);

                return result;
            }
            else
            {
                if (result == null)
                {
                    result = new HashSet<EventBean>();
                }
                foreach (var entry in sortedMapOne)
                {
                    next.Add(eventsPerStream, entry.Value as IDictionary<object, object>, result, postProcessor);
                }
                if (sortedMapTwo != null)
                {
                    foreach (var entry in sortedMapTwo)
                    {
                        next.Add(eventsPerStream, entry.Value as IDictionary<object, object>, result, postProcessor);
                    }
                }
                return result;
            }
        }

        private static void AddResults(
            IEnumerable<KeyValuePair<object, ICollection<EventBean>>> sortedMapOne,
            IEnumerable<KeyValuePair<object, ICollection<EventBean>>> sortedMapTwo,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            AddResults(sortedMapOne, result, postProcessor);
            if (sortedMapTwo != null)
            {
                AddResults(sortedMapTwo, result, postProcessor);
            }
        }

        private static void AddResults(
            IEnumerable<KeyValuePair<object, ICollection<EventBean>>> sortedMapOne,
            ICollection<EventBean> result,
            CompositeIndexQueryResultPostProcessor postProcessor)
        {
            if (postProcessor != null)
            {
                foreach (var entry in sortedMapOne)
                {
                    postProcessor.Add(entry.Value, result);
                }
            }
            else
            {
                foreach (var entry in sortedMapOne)
                {
                    result.AddAll(entry.Value);
                }
            }
        }

        public void SetNext(CompositeIndexQuery next)
        {
            _next = next;
        }
    }
} // end of namespace
