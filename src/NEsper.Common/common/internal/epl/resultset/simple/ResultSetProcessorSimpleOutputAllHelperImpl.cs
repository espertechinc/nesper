///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.simple
{
    public class ResultSetProcessorSimpleOutputAllHelperImpl : ResultSetProcessorSimpleOutputAllHelper
    {
        private readonly Deque<MultiKeyArrayOfKeys<EventBean>> _eventsNewJoin = new ArrayDeque<MultiKeyArrayOfKeys<EventBean>>(2);

        private readonly Deque<EventBean> _eventsNewView = new ArrayDeque<EventBean>(2);
        private readonly Deque<MultiKeyArrayOfKeys<EventBean>> _eventsOldJoin = new ArrayDeque<MultiKeyArrayOfKeys<EventBean>>(2);
        private readonly Deque<EventBean> _eventsOldView = new ArrayDeque<EventBean>(2);
        private readonly ResultSetProcessorSimple _processor;

        public ResultSetProcessorSimpleOutputAllHelperImpl(ResultSetProcessorSimple processor)
        {
            _processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (!_processor.HasHavingClause) {
                AddToView(newData, oldData);
                return;
            }

            var eventsPerStream = new EventBean[1];
            if (newData != null && newData.Length > 0) {
                foreach (var theEvent in newData) {
                    eventsPerStream[0] = theEvent;

                    var passesHaving = _processor.EvaluateHavingClause(
                        eventsPerStream,
                        true,
                        _processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    _eventsNewView.Add(theEvent);
                }
            }

            if (oldData != null && oldData.Length > 0) {
                foreach (var theEvent in oldData) {
                    eventsPerStream[0] = theEvent;

                    var passesHaving = _processor.EvaluateHavingClause(
                        eventsPerStream,
                        false,
                        _processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    _eventsOldView.Add(theEvent);
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            if (!_processor.HasHavingClause) {
                AddToJoin(newEvents, oldEvents);
                return;
            }

            if (newEvents != null && newEvents.Count > 0) {
                foreach (var theEvent in newEvents) {
                    var passesHaving = _processor.EvaluateHavingClause(
                        theEvent.Array,
                        true,
                        _processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    _eventsNewJoin.Add(theEvent);
                }
            }

            if (oldEvents != null && oldEvents.Count > 0) {
                foreach (var theEvent in oldEvents) {
                    var passesHaving = _processor.EvaluateHavingClause(
                        theEvent.Array,
                        false,
                        _processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    _eventsOldJoin.Add(theEvent);
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            var pair = _processor.ProcessViewResult(
                EventBeanUtility.ToArrayNullIfEmpty(_eventsNewView),
                EventBeanUtility.ToArrayNullIfEmpty(_eventsOldView),
                isSynthesize);
            _eventsNewView.Clear();
            _eventsOldView.Clear();
            return pair;
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            var pair = _processor.ProcessJoinResult(
                EventBeanUtility.ToLinkedHashSetNullIfEmpty(_eventsNewJoin),
                EventBeanUtility.ToLinkedHashSetNullIfEmpty(_eventsOldJoin),
                isSynthesize);
            _eventsNewJoin.Clear();
            _eventsOldJoin.Clear();
            return pair;
        }

        public void Destroy()
        {
            // no action required
        }

        private void AddToView(
            EventBean[] newData,
            EventBean[] oldData)
        {
            EventBeanUtility.AddToCollection(newData, _eventsNewView);
            EventBeanUtility.AddToCollection(oldData, _eventsOldView);
        }

        private void AddToJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            EventBeanUtility.AddToCollection(newEvents, _eventsNewJoin);
            EventBeanUtility.AddToCollection(oldEvents, _eventsOldJoin);
        }
    }
} // end of namespace