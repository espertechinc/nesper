///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor for the case: aggregation functions used in the select clause, and no group-by,
    /// and not all of the properties in the select clause are under an aggregation function.
    /// <para />This processor does not perform grouping, every event entering and leaving is in the same group.
    /// The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
    /// Aggregation state is simply one row holding all the state.
    /// </summary>
    public class ResultSetProcessorAggregateAll : ResultSetProcessor
    {
        private readonly ResultSetProcessorAggregateAllFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService;
        private ExprEvaluatorContext _exprEvaluatorContext;
        private readonly ResultSetProcessorAggregateAllOutputLastHelper _outputLastUnordHelper;
        private readonly ResultSetProcessorAggregateAllOutputAllHelper _outputAllUnordHelper;

        public ResultSetProcessorAggregateAll(
            ResultSetProcessorAggregateAllFactory prototype,
            SelectExprProcessor selectExprProcessor,
            OrderByProcessor orderByProcessor,
            AggregationService aggregationService,
            AgentInstanceContext agentInstanceContext)
        {
            _prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            _aggregationService = aggregationService;
            _exprEvaluatorContext = agentInstanceContext;
            _outputLastUnordHelper = prototype.IsEnableOutputLimitOpt && prototype.IsOutputLast ? prototype.ResultSetProcessorHelperFactory.MakeRSAggregateAllOutputLast(this, agentInstanceContext) : null;
            _outputAllUnordHelper = prototype.IsEnableOutputLimitOpt && prototype.IsOutputAll ? prototype.ResultSetProcessorHelperFactory.MakeRSAggregateAllOutputAll(this, agentInstanceContext) : null;
        }

        public AgentInstanceContext AgentInstanceContext
        {
            set { _exprEvaluatorContext = value; }
        }

        public EventType ResultEventType
        {
            get { return _prototype.ResultEventType; }
        }

        public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            var eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, eventsPerStream);
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
        }

        public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessUngroupedNonfullyAgg(); }
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;

            if (_prototype.IsUnidirectional)
            {
                Clear();
            }

            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);

            if (_prototype.OptionalHavingNode == null)
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, oldEvents, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, false, isSynthesize, _exprEvaluatorContext);
                    }
                }

                if (_orderByProcessor == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, newEvents, true, isSynthesize, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, true, isSynthesize, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, oldEvents, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                }

                if (_orderByProcessor == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, newEvents, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
            }

            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(null, null); }
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(selectNewEvents, selectOldEvents); }
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }

        public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessUngroupedNonfullyAgg(); }
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;

            var eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, eventsPerStream);

            // generate new events using select expressions
            if (_prototype.OptionalHavingNode == null)
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, isSynthesize, _exprEvaluatorContext);
                    }
                }

                if (_orderByProcessor == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, isSynthesize, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, isSynthesize, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                }

                if (_orderByProcessor == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
            }

            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(null, null); }
                return null;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(selectNewEvents, selectOldEvents); }
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }

        public static readonly IList<EventBean> EMPTY_EVENT_BEAN_LIST = new EventBean[0];

        public IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!_prototype.IsHistoricalOnly)
            {
                return ObtainEnumerator(parent);
            }

            ResultSetProcessorUtil.ClearAndAggregateUngrouped(_exprEvaluatorContext, _aggregationService, parent);
            ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainEnumerator(parent));
            _aggregationService.ClearResults(_exprEvaluatorContext);
            return deque.GetEnumerator();
        }

        public IEnumerator<EventBean> ObtainEnumerator(Viewable parent)
        {
            if (_orderByProcessor == null)
            {
                return ResultSetAggregateAllEnumerator.New(parent, this, _exprEvaluatorContext);
            }

            // Pull all parent events, generate order keys
            var eventsPerStream = new EventBean[1];
            var outgoingEvents = new List<EventBean>();
            var orderKeys = new List<object>();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, _exprEvaluatorContext);

            foreach (var candidate in parent)
            {
                eventsPerStream[0] = candidate;

                object pass = true;
                if (_prototype.OptionalHavingNode != null)
                {
                    pass = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
                }
                if ((pass == null) || (false.Equals(pass)))
                {
                    continue;
                }

                outgoingEvents.Add(_selectExprProcessor.Process(eventsPerStream, true, true, _exprEvaluatorContext));

                var orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, _exprEvaluatorContext);
                orderKeys.Add(orderKey);
            }

            // sort
            var outgoingEventsArr = outgoingEvents.ToArray();
            var orderKeysArr = orderKeys.ToArray();
            var orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, _exprEvaluatorContext) ?? EMPTY_EVENT_BEAN_LIST;

            return orderedEvents.GetEnumerator();
        }

        /// <summary>
        /// Returns the select expression processor
        /// </summary>
        /// <value>select processor.</value>
        public SelectExprProcessor SelectExprProcessor
        {
            get { return _selectExprProcessor; }
        }

        /// <summary>
        /// Returns the optional having expression.
        /// </summary>
        /// <value>having expression node</value>
        public ExprEvaluator OptionalHavingNode
        {
            get { return _prototype.OptionalHavingNode; }
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            IList<EventBean> result;
            if (_prototype.OptionalHavingNode == null)
            {
                if (_orderByProcessor == null)
                {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, joinSet, true, true, _exprEvaluatorContext);
                }
                else
                {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, joinSet, true, true, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_orderByProcessor == null)
                {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, joinSet, _prototype.OptionalHavingNode, true, true, _exprEvaluatorContext);
                }
                else
                {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, joinSet, _prototype.OptionalHavingNode, true, true, _exprEvaluatorContext);
                }
            }
            return result != null ? result.GetEnumerator() : EnumerationHelper.Empty<EventBean>();
        }

        public void Clear()
        {
            _aggregationService.ClearResults(_exprEvaluatorContext);
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.LAST)
            {
                return ProcessOutputLimitedJoinLast(joinEventsSet, generateSynthetic);
            }
            else
            {
                return ProcessOutputLimitedJoinDefault(joinEventsSet, generateSynthetic);
            }
        }

        public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.LAST)
            {
                return ProcessOutputLimitedViewLast(viewEventsList, generateSynthetic);
            }
            else
            {
                return ProcessOutputLimitedViewDefault(viewEventsList, generateSynthetic);
            }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllUnordHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
            else
            {
                _outputLastUnordHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllUnordHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
            }
            else
            {
                _outputLastUnordHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
            }
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllUnordHelper.Output();
            }
            return _outputLastUnordHelper.Output();
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllUnordHelper.Output();
            }
            return _outputLastUnordHelper.Output();
        }

        public void Stop()
        {
            if (_outputLastUnordHelper != null)
            {
                _outputLastUnordHelper.Destroy();
            }
            if (_outputAllUnordHelper != null)
            {
                _outputAllUnordHelper.Destroy();
            }
        }

        private UniformPair<EventBean[]> ProcessOutputLimitedJoinDefault(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
            IList<EventBean> newEvents = new List<EventBean>();
            IList<EventBean> oldEvents = null;
            if (_prototype.IsSelectRStream)
            {
                oldEvents = new List<EventBean>();
            }

            IList<object> newEventsSortKey = null;
            IList<object> oldEventsSortKey = null;
            if (_orderByProcessor != null)
            {
                newEventsSortKey = new List<object>();
                if (_prototype.IsSelectRStream)
                {
                    oldEventsSortKey = new List<object>();
                }
            }

            foreach (var pair in joinEventsSet)
            {
                var newData = pair.First;
                var oldData = pair.Second;

                if (_prototype.IsUnidirectional)
                {
                    Clear();
                }

                if (newData != null)
                {
                    // apply new data to aggregates
                    foreach (var row in newData)
                    {
                        _aggregationService.ApplyEnter(row.Array, null, _exprEvaluatorContext);
                    }
                }
                if (oldData != null)
                {
                    // apply old data to aggregates
                    foreach (var row in oldData)
                    {
                        _aggregationService.ApplyLeave(row.Array, null, _exprEvaluatorContext);
                    }
                }

                // generate old events using select expressions
                if (_prototype.IsSelectRStream)
                {
                    if (_prototype.OptionalHavingNode == null)
                    {
                        if (_orderByProcessor == null)
                        {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                        }
                        else
                        {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                    // generate old events using having then select
                    else
                    {
                        if (_orderByProcessor == null)
                        {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                        }
                        else
                        {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                }

                // generate new events using select expressions
                if (_prototype.OptionalHavingNode == null)
                {
                    if (_orderByProcessor == null)
                    {
                        ResultSetProcessorUtil.PopulateSelectJoinEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                    }
                    else
                    {
                        ResultSetProcessorUtil.PopulateSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                    }
                }
                else
                {
                    if (_orderByProcessor == null)
                    {
                        ResultSetProcessorUtil.PopulateSelectJoinEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                    }
                    else
                    {
                        ResultSetProcessorUtil.PopulateSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                    }
                }
            }

            var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
            EventBean[] oldEventsArr = null;
            if (_prototype.IsSelectRStream)
            {
                oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
            }

            if (_orderByProcessor != null)
            {
                var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);
                if (_prototype.IsSelectRStream)
                {
                    var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
                    oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _exprEvaluatorContext);
                }
            }

            if ((newEventsArr == null) && (oldEventsArr == null))
            {
                return null;
            }
            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }

        private UniformPair<EventBean[]> ProcessOutputLimitedJoinLast(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
            EventBean lastOldEvent = null;
            EventBean lastNewEvent = null;

            foreach (var pair in joinEventsSet)
            {
                var newData = pair.First;
                var oldData = pair.Second;

                if (_prototype.IsUnidirectional)
                {
                    Clear();
                }

                if (newData != null)
                {
                    // apply new data to aggregates
                    foreach (var eventsPerStream in newData)
                    {
                        _aggregationService.ApplyEnter(eventsPerStream.Array, null, _exprEvaluatorContext);
                    }
                }
                if (oldData != null)
                {
                    // apply old data to aggregates
                    foreach (var eventsPerStream in oldData)
                    {
                        _aggregationService.ApplyLeave(eventsPerStream.Array, null, _exprEvaluatorContext);
                    }
                }

                EventBean[] selectOldEvents;
                if (_prototype.IsSelectRStream)
                {
                    if (_prototype.OptionalHavingNode == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, _exprEvaluatorContext);
                    }
                    if ((selectOldEvents != null) && (selectOldEvents.Length > 0))
                    {
                        lastOldEvent = selectOldEvents[selectOldEvents.Length - 1];
                    }
                }

                // generate new events using select expressions
                EventBean[] selectNewEvents;
                if (_prototype.OptionalHavingNode == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, _exprEvaluatorContext);
                }
                if ((selectNewEvents != null) && (selectNewEvents.Length > 0))
                {
                    lastNewEvent = selectNewEvents[selectNewEvents.Length - 1];
                }
            }

            var lastNew = (lastNewEvent != null) ? new EventBean[] { lastNewEvent } : null;
            var lastOld = (lastOldEvent != null) ? new EventBean[] { lastOldEvent } : null;

            if ((lastNew == null) && (lastOld == null))
            {
                return null;
            }
            return new UniformPair<EventBean[]>(lastNew, lastOld);
        }

        private UniformPair<EventBean[]> ProcessOutputLimitedViewDefault(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            IList<EventBean> newEvents = new List<EventBean>();
            IList<EventBean> oldEvents = null;
            if (_prototype.IsSelectRStream)
            {
                oldEvents = new List<EventBean>();
            }
            IList<object> newEventsSortKey = null;
            IList<object> oldEventsSortKey = null;
            if (_orderByProcessor != null)
            {
                newEventsSortKey = new List<object>();
                if (_prototype.IsSelectRStream)
                {
                    oldEventsSortKey = new List<object>();
                }
            }

            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;

                var eventsPerStream = new EventBean[1];
                if (newData != null)
                {
                    // apply new data to aggregates
                    foreach (var aNewData in newData)
                    {
                        eventsPerStream[0] = aNewData;
                        _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
                    }
                }
                if (oldData != null)
                {
                    // apply old data to aggregates
                    foreach (var anOldData in oldData)
                    {
                        eventsPerStream[0] = anOldData;
                        _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
                    }
                }

                // generate old events using select expressions
                if (_prototype.IsSelectRStream)
                {
                    if (_prototype.OptionalHavingNode == null)
                    {
                        if (_orderByProcessor == null)
                        {
                            ResultSetProcessorUtil.PopulateSelectEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                        }
                        else
                        {
                            ResultSetProcessorUtil.PopulateSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                    // generate old events using having then select
                    else
                    {
                        if (_orderByProcessor == null)
                        {
                            ResultSetProcessorUtil.PopulateSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                        }
                        else
                        {
                            ResultSetProcessorUtil.PopulateSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                }

                // generate new events using select expressions
                if (_prototype.OptionalHavingNode == null)
                {
                    if (_orderByProcessor == null)
                    {
                        ResultSetProcessorUtil.PopulateSelectEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                    }
                    else
                    {
                        ResultSetProcessorUtil.PopulateSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                    }
                }
                else
                {
                    if (_orderByProcessor == null)
                    {
                        ResultSetProcessorUtil.PopulateSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                    }
                    else
                    {
                        ResultSetProcessorUtil.PopulateSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                    }
                }
            }

            var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
            EventBean[] oldEventsArr = null;
            if (_prototype.IsSelectRStream)
            {
                oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
            }
            if (_orderByProcessor != null)
            {
                var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);

                if (_prototype.IsSelectRStream)
                {
                    var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
                    oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _exprEvaluatorContext);
                }
            }

            if ((newEventsArr == null) && (oldEventsArr == null))
            {
                return null;
            }
            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }

        private UniformPair<EventBean[]> ProcessOutputLimitedViewLast(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            EventBean lastOldEvent = null;
            EventBean lastNewEvent = null;
            var eventsPerStream = new EventBean[1];

            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;

                if (newData != null)
                {
                    // apply new data to aggregates
                    foreach (var aNewData in newData)
                    {
                        eventsPerStream[0] = aNewData;
                        _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
                    }
                }
                if (oldData != null)
                {
                    // apply old data to aggregates
                    foreach (var anOldData in oldData)
                    {
                        eventsPerStream[0] = anOldData;
                        _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
                    }
                }

                EventBean[] selectOldEvents;
                if (_prototype.IsSelectRStream)
                {
                    if (_prototype.OptionalHavingNode == null)
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, _exprEvaluatorContext);
                    }
                    if ((selectOldEvents != null) && (selectOldEvents.Length > 0))
                    {
                        lastOldEvent = selectOldEvents[selectOldEvents.Length - 1];
                    }
                }

                // generate new events using select expressions
                EventBean[] selectNewEvents;
                if (_prototype.OptionalHavingNode == null)
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, _exprEvaluatorContext);
                }
                else
                {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, _exprEvaluatorContext);
                }
                if ((selectNewEvents != null) && (selectNewEvents.Length > 0))
                {
                    lastNewEvent = selectNewEvents[selectNewEvents.Length - 1];
                }
            }

            var lastNew = (lastNewEvent != null) ? new EventBean[] { lastNewEvent } : null;
            var lastOld = (lastOldEvent != null) ? new EventBean[] { lastOldEvent } : null;

            if ((lastNew == null) && (lastOld == null))
            {
                return null;
            }
            return new UniformPair<EventBean[]>(lastNew, lastOld);
        }

        public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
            if (_outputLastUnordHelper != null)
            {
                visitor.Visit(_outputLastUnordHelper);
            }
            if (_outputAllUnordHelper != null)
            {
                visitor.Visit(_outputAllUnordHelper);
            }
        }
    }
} // end of namespace
