///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor for the simplest case: no aggregation functions used in the 
    /// select clause, and no group-by.
    /// <para/>
    /// The processor generates one row for each event entering (new event) and one row 
    /// for each event leaving (old event).
    /// </summary>
    public class ResultSetProcessorSimple : ResultSetProcessorBaseSimple
    {
        internal readonly ResultSetProcessorSimpleFactory Prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        internal ExprEvaluatorContext ExprEvaluatorContext;
        private ResultSetProcessorSimpleOutputLastHelper _outputLastHelper;
        private ResultSetProcessorSimpleOutputAllHelper _outputAllHelper;

        public ResultSetProcessorSimple(
            ResultSetProcessorSimpleFactory prototype,
            SelectExprProcessor selectExprProcessor,
            OrderByProcessor orderByProcessor,
            AgentInstanceContext agentInstanceContext)
        {
            Prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            ExprEvaluatorContext = agentInstanceContext;
            if (prototype.IsOutputLast)
            {
                // output-last always uses this mechanism
                _outputLastHelper = prototype.ResultSetProcessorHelperFactory
                    .MakeRSSimpleOutputLast(prototype, this, agentInstanceContext);
            }
            else if (prototype.IsOutputAll && prototype.IsEnableOutputLimitOpt)
            {
                _outputAllHelper = prototype.ResultSetProcessorHelperFactory
                    .MakeRSSimpleOutputAll(prototype, this, agentInstanceContext);
            }
        }


        public override AgentInstanceContext AgentInstanceContext
        {
            set { ExprEvaluatorContext = value; }
            get { return (AgentInstanceContext) ExprEvaluatorContext; }
        }

        public override EventType ResultEventType
        {
            get { return Prototype.ResultEventType; }
        }

        public override UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessSimple();}
    
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
    
            if (Prototype.OptionalHavingExpr == null)
            {
                if (Prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, oldEvents, false, isSynthesize, ExprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, false, isSynthesize, ExprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, newEvents, true, isSynthesize, ExprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, true, isSynthesize, ExprEvaluatorContext);
                }
            }
            else
            {
                if (Prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, oldEvents, Prototype.OptionalHavingExpr, false, isSynthesize, ExprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, Prototype.OptionalHavingExpr, false, isSynthesize, ExprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, newEvents, Prototype.OptionalHavingExpr, true, isSynthesize, ExprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, Prototype.OptionalHavingExpr, true, isSynthesize, ExprEvaluatorContext);
                }
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessSimple(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        public override UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessSimple();}
    
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
            if (Prototype.OptionalHavingExpr == null)
            {
                if (Prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, isSynthesize, ExprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, isSynthesize, ExprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, isSynthesize, ExprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, isSynthesize, ExprEvaluatorContext);
                }
            }
            else
            {
                if (Prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, oldData, Prototype.OptionalHavingExpr, false, isSynthesize, ExprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, Prototype.OptionalHavingExpr, false, isSynthesize, ExprEvaluatorContext);
                    }
                }
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, Prototype.OptionalHavingExpr, true, isSynthesize, ExprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, Prototype.OptionalHavingExpr, true, isSynthesize, ExprEvaluatorContext);
                }
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessSimple(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        /// <summary>Process view results for the iterator. </summary>
        /// <param name="newData">new events</param>
        /// <returns>pair of insert and remove stream</returns>
        public UniformPair<EventBean[]> ProcessViewResultIterator(EventBean[] newData)
        {
            EventBean[] selectNewEvents;
            if (Prototype.OptionalHavingExpr == null)
            {
                // ignore _orderByProcessor
                selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, true, ExprEvaluatorContext);
            }
            else
            {
                // ignore _orderByProcessor
                selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, Prototype.OptionalHavingExpr, true, true, ExprEvaluatorContext);
            }
    
            return new UniformPair<EventBean[]>(selectNewEvents, null);
        }
    
        public override IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (_orderByProcessor != null)
            {
                // Pull all events, generate order keys
                var eventsPerStream = new EventBean[1];
                var events = new List<EventBean>();
                var orderKeys = new List<Object>();

                var parentEnumerator = parent.GetEnumerator();
                if (parentEnumerator.MoveNext() == false)
                {
                    return CollectionUtil.NULL_EVENT_ITERATOR;
                }

                do
                {
                    var aParent = parentEnumerator.Current;
                    eventsPerStream[0] = aParent;
                    var orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, ExprEvaluatorContext);
                    var pair = ProcessViewResultIterator(eventsPerStream);
                    var result = pair.First;
                    if (result != null && result.Length != 0)
                    {
                        events.Add(result[0]);
                    }
                    orderKeys.Add(orderKey);
                } while (parentEnumerator.MoveNext());
    
                // sort
                var outgoingEvents = events.ToArray();
                var orderKeysArr = orderKeys.ToArray();
                var orderedEvents = _orderByProcessor.Sort(outgoingEvents, orderKeysArr, ExprEvaluatorContext);
                if (orderedEvents == null)
                    return EnumerationHelper.Empty<EventBean>();

                return ((IEnumerable<EventBean>)orderedEvents).GetEnumerator();
            }
            // Return an iterator that gives row-by-row a result

            var transform = new ResultSetProcessorSimpleTransform(this);
            return parent.Select(transform.Transform).GetEnumerator();
        }

        public override IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            // Process join results set as a regular join, includes sorting and having-clause filter
            var result = ProcessJoinResult(joinSet, CollectionUtil.EMPTY_ROW_SET, true);
            var first = result.First;
            if (first == null)
                return EnumerationHelper.Empty<EventBean>();
            return ((IEnumerable<EventBean>)first).GetEnumerator();
        }
    
        public override void Clear()
        {
            // No need to clear state, there is no state held
        }

        public override bool HasAggregation
        {
            get { return false; }
        }

        public override void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
        }

        public override void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
        }

        public override void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllHelper.ProcessView(newData, oldData);
            }
            else
            {
                _outputLastHelper.ProcessView(newData, oldData);
            }
        }

        public override void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllHelper.ProcessJoin(newEvents, oldEvents);
            }
            else
            {
                _outputLastHelper.ProcessJoin(newEvents, oldEvents);
            }
        }

        public override UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllHelper.OutputView(isSynthesize);
            }
            return _outputLastHelper.OutputView(isSynthesize);
        }

        public override UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllHelper.OutputJoin(isSynthesize);
            }
            return _outputLastHelper.OutputJoin(isSynthesize);
        }

        public override void Stop()
        {
            if (_outputLastHelper != null)
            {
                _outputLastHelper.Destroy();
            }
            if (_outputAllHelper != null)
            {
                _outputAllHelper.Destroy();
            }
        }

        public override void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
            if (_outputLastHelper != null)
            {
                visitor.Visit(_outputLastHelper);
            }
            if (_outputAllHelper != null)
            {
                visitor.Visit(_outputAllHelper);
            }
        }
    }
}
