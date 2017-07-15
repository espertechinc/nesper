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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// An order-by processor that sorts events according to the expressions
    /// in the order_by clause.
    /// </summary>
    public class OrderByProcessorImpl : OrderByProcessor
    {
        private readonly OrderByProcessorFactoryImpl _factory;
        private readonly AggregationService _aggregationService;

        public OrderByProcessorImpl(OrderByProcessorFactoryImpl factory, AggregationService aggregationService)
        {
            _factory = factory;
            _aggregationService = aggregationService;
        }

        public Object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return GetSortKeyInternal(eventsPerStream, isNewData, exprEvaluatorContext, _factory.OrderBy);
        }

        public Object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, OrderByElement[] elementsForLevel)
        {
            return GetSortKeyInternal(eventsPerStream, isNewData, exprEvaluatorContext, elementsForLevel);
        }

        private static Object GetSortKeyInternal(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, OrderByElement[] elements)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(eventsPerStream, elements); }

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            if (elements.Length == 1)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    var value = elements[0].Expr.Evaluate(evaluateParams);
                    InstrumentationHelper.Get().AOrderBy(value);
                    return value;
                }
                return elements[0].Expr.Evaluate(evaluateParams);
            }

            var values = new Object[elements.Length];
            var count = 0;
            foreach (var sortPair in elements)
            {
                values[count++] = sortPair.Expr.Evaluate(evaluateParams);
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(values); }
            return new MultiKeyUntyped(values);
        }

        public Object[] GetSortKeyPerRow(EventBean[] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (generatingEvents == null)
            {
                return null;
            }

            var sortProperties = new Object[generatingEvents.Length];

            var count = 0;
            var evalEventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(evalEventsPerStream, isNewData, exprEvaluatorContext);

            if (_factory.OrderBy.Length == 1)
            {
                var singleEval = _factory.OrderBy[0].Expr;
                foreach (var theEvent in generatingEvents)
                {
                    evalEventsPerStream[0] = theEvent;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(evalEventsPerStream, _factory.OrderBy); }
                    sortProperties[count] = singleEval.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(sortProperties[count]); }
                    count++;
                }
            }
            else
            {
                foreach (var theEvent in generatingEvents)
                {
                    var values = new Object[_factory.OrderBy.Length];
                    var countTwo = 0;
                    evalEventsPerStream[0] = theEvent;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(evalEventsPerStream, _factory.OrderBy); }
                    foreach (var sortPair in _factory.OrderBy)
                    {
                        values[countTwo++] = sortPair.Expr.Evaluate(evaluateParams);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(values); }

                    sortProperties[count] = new MultiKeyUntyped(values);
                    count++;
                }
            }

            return sortProperties;
        }

        public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (outgoingEvents == null || outgoingEvents.Length < 2)
            {
                return outgoingEvents;
            }

            // Get the group by keys if needed
            Object[] groupByKeys = null;
            if (_factory.IsNeedsGroupByKeys)
            {
                groupByKeys = GenerateGroupKeys(generatingEvents, isNewData, exprEvaluatorContext);
            }

            return Sort(outgoingEvents, generatingEvents, groupByKeys, isNewData, exprEvaluatorContext);
        }

        public EventBean[] Sort(EventBean[] outgoingEvents, IList<GroupByRollupKey> currentGenerators, bool isNewData, AgentInstanceContext exprEvaluatorContext, OrderByElement[][] elementsPerLevel)
        {
            var sortValuesMultiKeys = CreateSortPropertiesWRollup(currentGenerators, elementsPerLevel, isNewData, exprEvaluatorContext);
            return SortInternal(outgoingEvents, sortValuesMultiKeys, _factory.Comparator);
        }

        public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, Object[] groupByKeys, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (outgoingEvents == null || outgoingEvents.Length < 2)
            {
                return outgoingEvents;
            }

            // Create the multikeys of sort values
            var sortValuesMultiKeys = CreateSortProperties(generatingEvents, groupByKeys, isNewData, exprEvaluatorContext);

            return SortInternal(outgoingEvents, sortValuesMultiKeys, _factory.Comparator);
        }

        private IList<Object> CreateSortProperties(EventBean[][] generatingEvents, Object[] groupByKeys, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var sortProperties = new Object[generatingEvents.Length];

            var elements = _factory.OrderBy;
            if (elements.Length == 1)
            {
                var count = 0;
                foreach (var eventsPerStream in generatingEvents)
                {
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

                    // Make a new multikey that contains the sort-by values.
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(groupByKeys[count], exprEvaluatorContext.AgentInstanceId, null);
                    }

                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(eventsPerStream, _factory.OrderBy); }
                    sortProperties[count] = elements[0].Expr.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(sortProperties[count]); }
                    count++;
                }
            }
            else
            {
                var count = 0;
                foreach (var eventsPerStream in generatingEvents)
                {
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

                    // Make a new multikey that contains the sort-by values.
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(groupByKeys[count], exprEvaluatorContext.AgentInstanceId, null);
                    }

                    var values = new Object[_factory.OrderBy.Length];
                    var countTwo = 0;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(eventsPerStream, _factory.OrderBy); }
                    foreach (var sortPair in _factory.OrderBy)
                    {
                        values[countTwo++] = sortPair.Expr.Evaluate(evaluateParams);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(values); }

                    sortProperties[count] = new MultiKeyUntyped(values);
                    count++;
                }
            }

            return sortProperties;
        }

        public EventBean[] Sort(EventBean[] outgoingEvents, Object[] orderKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            var sort = new SortedDictionary<Object, Object>(_factory.Comparator).WithNullSupport();

            if (outgoingEvents == null || outgoingEvents.Length < 2)
            {
                return outgoingEvents;
            }

            for (var i = 0; i < outgoingEvents.Length; i++)
            {
                var entry = sort.Get(orderKeys[i]);
                if (entry == null)
                {
                    sort.Put(orderKeys[i], outgoingEvents[i]);
                }
                else if (entry is EventBean)
                {
                    IList<EventBean> list = new List<EventBean>();
                    list.Add((EventBean)entry);
                    list.Add(outgoingEvents[i]);
                    sort.Put(orderKeys[i], list);
                }
                else
                {
                    var list = (IList<EventBean>)entry;
                    list.Add(outgoingEvents[i]);
                }
            }

            var result = new EventBean[outgoingEvents.Length];
            var count = 0;
            foreach (Object entry in sort.Values)
            {
                if (entry is IList<EventBean>)
                {
                    var output = (IList<EventBean>)entry;
                    foreach (var theEvent in output)
                    {
                        result[count++] = theEvent;
                    }
                }
                else
                {
                    result[count++] = (EventBean)entry;
                }
            }
            return result;
        }

        private Object[] GenerateGroupKeys(EventBean[][] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var keys = new Object[generatingEvents.Length];

            var count = 0;
            foreach (var eventsPerStream in generatingEvents)
            {
                keys[count++] = GenerateGroupKey(eventsPerStream, isNewData, exprEvaluatorContext);
            }

            return keys;
        }

        private Object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            var evals = _factory.GroupByNodes;
            if (evals.Length == 1)
            {
                return evals[0].Evaluate(evaluateParams);
            }

            var keys = new Object[evals.Length];
            var count = 0;
            foreach (var exprNode in evals)
            {
                keys[count] = exprNode.Evaluate(evaluateParams);
                count++;
            }

            return new MultiKeyUntyped(keys);
        }

        private IList<Object> CreateSortPropertiesWRollup(IList<GroupByRollupKey> currentGenerators, OrderByElement[][] elementsPerLevel, bool isNewData, AgentInstanceContext exprEvaluatorContext)
        {
            var sortProperties = new Object[currentGenerators.Count];

            var elements = _factory.OrderBy;
            if (elements.Length == 1)
            {
                var count = 0;
                foreach (var rollup in currentGenerators)
                {

                    // Make a new multikey that contains the sort-by values.
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(rollup.GroupKey, exprEvaluatorContext.AgentInstanceId, rollup.Level);
                    }

                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(rollup.Generator, _factory.OrderBy); }
                    sortProperties[count] = elementsPerLevel[rollup.Level.LevelNumber][0].Expr.Evaluate(
                        new EvaluateParams(rollup.Generator, isNewData, exprEvaluatorContext));
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(sortProperties[count]); }

                    count++;
                }
            }
            else
            {
                var count = 0;
                foreach (var rollup in currentGenerators)
                {
                    var evaluateParams = new EvaluateParams(rollup.Generator, isNewData, exprEvaluatorContext);

                    // Make a new multikey that contains the sort-by values.
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(rollup.GroupKey, exprEvaluatorContext.AgentInstanceId, rollup.Level);
                    }

                    var values = new Object[_factory.OrderBy.Length];
                    var countTwo = 0;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(rollup.Generator, _factory.OrderBy); }
                    foreach (var sortPair in elementsPerLevel[rollup.Level.LevelNumber])
                    {
                        values[countTwo++] = sortPair.Expr.Evaluate(evaluateParams);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(values); }

                    sortProperties[count] = new MultiKeyUntyped(values);
                    count++;
                }
            }
            return sortProperties;
        }

        private static EventBean[] SortInternal(EventBean[] outgoingEvents, IList<Object> sortValuesMultiKeys, IComparer<Object> comparator)
        {
            // Map the sort values to the corresponding outgoing events
            var sortToOutgoing = new Dictionary<Object, IList<EventBean>>().WithNullSupport();
            var countOne = 0;
            foreach (var sortValues in sortValuesMultiKeys)
            {
                var list = sortToOutgoing.Get(sortValues);
                if (list == null)
                {
                    list = new List<EventBean>();
                }
                list.Add(outgoingEvents[countOne++]);
                sortToOutgoing.Put(sortValues, list);
            }

            // Sort the sort values
            sortValuesMultiKeys.SortInPlace(comparator);

            // Sort the outgoing events in the same order
            var sortSet = new LinkedHashSet<Object>(sortValuesMultiKeys);
            var result = new EventBean[outgoingEvents.Length];
            var countTwo = 0;
            foreach (var sortValues in sortSet)
            {
                ICollection<EventBean> output = sortToOutgoing.Get(sortValues);
                foreach (var theEvent in output)
                {
                    result[countTwo++] = theEvent;
                }
            }

            return result;
        }

        public EventBean DetermineLocalMinMax(EventBean[] outgoingEvents, EventBean[][] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            // Get the group by keys if needed
            Object[] groupByKeys = null;
            if (_factory.IsNeedsGroupByKeys)
            {
                groupByKeys = GenerateGroupKeys(generatingEvents, isNewData, exprEvaluatorContext);
            }

            OrderByElement[] elements = _factory.OrderBy;
            Object localMinMax = null;
            EventBean outgoingMinMaxBean = null;

            if (elements.Length == 1)
            {
                int count = 0;
                foreach (EventBean[] eventsPerStream in generatingEvents)
                {
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(groupByKeys[count], exprEvaluatorContext.AgentInstanceId, null);
                    }

                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(eventsPerStream, _factory.OrderBy); }
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                    var sortKey = elements[0].Expr.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(localMinMax); }

                    var newMinMax = localMinMax == null || _factory.Comparator.Compare(localMinMax, sortKey) > 0;
                    if (newMinMax)
                    {
                        localMinMax = sortKey;
                        outgoingMinMaxBean = outgoingEvents[count];
                    }

                    count++;
                }
            }
            else
            {
                var count = 0;
                var values = new Object[_factory.OrderBy.Length];
                var valuesMk = new MultiKeyUntyped(values);

                foreach (var eventsPerStream in generatingEvents)
                {
                    if (_factory.IsNeedsGroupByKeys)
                    {
                        _aggregationService.SetCurrentAccess(groupByKeys[count], exprEvaluatorContext.AgentInstanceId, null);
                    }

                    var countTwo = 0;
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOrderBy(eventsPerStream, _factory.OrderBy); }
                    foreach (var sortPair in _factory.OrderBy)
                    {
                        values[countTwo++] = sortPair.Expr.Evaluate(evaluateParams);
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOrderBy(values); }

                    var newMinMax = localMinMax == null || _factory.Comparator.Compare(localMinMax, valuesMk) > 0;
                    if (newMinMax)
                    {
                        localMinMax = valuesMk;
                        values = new Object[_factory.OrderBy.Length];
                        valuesMk = new MultiKeyUntyped(values);
                        outgoingMinMaxBean = outgoingEvents[count];
                    }

                    count++;
                }
            }

            return outgoingMinMaxBean;
        }

        public EventBean DetermineLocalMinMax(EventBean[] outgoingEvents, Object[] orderKeys)
        {
            Object localMinMax = null;
            EventBean outgoingMinMaxBean = null;

            for (int i = 0; i < outgoingEvents.Length; i++)
            {
                var newMinMax = localMinMax == null || _factory.Comparator.Compare(localMinMax, orderKeys[i]) > 0;
                if (newMinMax)
                {
                    localMinMax = orderKeys[i];
                    outgoingMinMaxBean = outgoingEvents[i];
                }
            }

            return outgoingMinMaxBean;
        }
    }
}
