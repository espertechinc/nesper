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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.exec.composite
{
    using Map = IDictionary<object, object>;

    public class CompositeIndexQueryRange : CompositeIndexQuery
    {
        private readonly CompositeAccessStrategy _strategy;
        private CompositeIndexQuery _next;
    
        public CompositeIndexQueryRange(bool isNWOnTrigger, int lookupStream, int numStreams, SubordPropRangeKey subqRangeKey, Type coercionType, IList<String> expressionTexts)
        {
            QueryGraphValueEntryRange rangeProp = subqRangeKey.RangeInfo;
    
            if (rangeProp.RangeType.IsRange())
            {
                var rangeIn = (QueryGraphValueEntryRangeIn) rangeProp;
                var start = rangeIn.ExprStart.ExprEvaluator;
                expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(rangeIn.ExprStart));
                var includeStart = rangeProp.RangeType.IsIncludeStart();
    
                var end = rangeIn.ExprEnd.ExprEvaluator;
                expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(rangeIn.ExprEnd));
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
                expressionTexts.Add(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(relOp.Expression));
                if (rangeProp.RangeType == QueryGraphRangeEnum.GREATER_OR_EQUAL) {
                    _strategy = new CompositeAccessStrategyGE(isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                }
                else if (rangeProp.RangeType == QueryGraphRangeEnum.GREATER) {
                    _strategy = new CompositeAccessStrategyGT(isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                }
                else if (rangeProp.RangeType == QueryGraphRangeEnum.LESS_OR_EQUAL) {
                    _strategy = new CompositeAccessStrategyLE(isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                }
                else if (rangeProp.RangeType == QueryGraphRangeEnum.LESS) {
                    _strategy = new CompositeAccessStrategyLT(isNWOnTrigger, lookupStream, numStreams, key, coercionType);
                }
                else {
                    throw new ArgumentException("Comparison operator " + rangeProp.RangeType + " not supported");
                }
            }
        }

        public void Add(EventBean theEvent, Map parent, ISet<EventBean> result)
        {
            _strategy.Lookup(theEvent, parent, result, _next, null, null);
        }

        public void Add(EventBean[] eventsPerStream, Map parent, ISet<EventBean> result)
        {
            _strategy.Lookup(eventsPerStream, parent, result, _next, null, null);
        }
    
        public ICollection<EventBean> Get(EventBean theEvent, Map parent, ExprEvaluatorContext context) {
            return _strategy.Lookup(theEvent, parent, null, _next, context, null);
        }
    
        public ICollection<EventBean> Get(EventBean[] eventsPerStream, Map parent, ExprEvaluatorContext context) {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, null);
        }
    
        public ISet<EventBean> GetCollectKeys(EventBean theEvent, Map parent, ExprEvaluatorContext context, IList<Object> keys) {
            return _strategy.Lookup(theEvent, parent, null, _next, context, keys);
        }

        public ISet<EventBean> GetCollectKeys(EventBean[] eventsPerStream, Map parent, ExprEvaluatorContext context, IList<Object> keys)
        {
            return _strategy.Lookup(eventsPerStream, parent, null, _next, context, keys);
        }

        internal static ISet<EventBean> Handle(
            EventBean theEvent,
            IDictionary<object, object> sortedMapOne,
            IDictionary<object, object> sortedMapTwo,
            ISet<EventBean> result,
            CompositeIndexQuery next)
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
                    result);

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
                    next.Add(theEvent, entry.Value as Map, result);
                }
                if (sortedMapTwo != null)
                {
                    foreach (var entry in sortedMapTwo)
                    {
                        next.Add(theEvent, entry.Value as Map, result);
                    }
                }
                return result;
            }
        }

        internal static ISet<EventBean> Handle(
            EventBean[] eventsPerStream,
            IDictionary<object, object> sortedMapOne,
            IDictionary<object, object> sortedMapTwo,
            ISet<EventBean> result,
            CompositeIndexQuery next)
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
                    result);

                return result;
            }
            else
            {
                if (result == null) {
                    result = new HashSet<EventBean>();
                }
                var map = sortedMapOne;
                foreach (var entry in map)
                {
                    next.Add(eventsPerStream, (Map) entry.Value, result);
                }
                if (sortedMapTwo != null) {
                    map = sortedMapTwo;
                    foreach (var entry in map) {
                        next.Add(eventsPerStream, (Map)entry.Value, result);
                    }
                }
                return result;
            }
        }

        private static void AddResults(
            IEnumerable<KeyValuePair<object, ICollection<EventBean>>> sortedMapOne,
            IEnumerable<KeyValuePair<object, ICollection<EventBean>>> sortedMapTwo,
            ISet<EventBean> result)
        {
            foreach (var entry in sortedMapOne)
            {
                result.AddAll(entry.Value);
            }

            if (sortedMapTwo != null)
            {
                foreach (var entry in sortedMapTwo)
                {
                    result.AddAll(entry.Value);
                }
            }
        }

        public CompositeIndexQuery Next
        {
            set { this._next = value; }
        }
    }
}
