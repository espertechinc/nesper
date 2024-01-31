///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly ResultSetProcessorSimple processor;

        private readonly Deque<EventBean> eventsNewView = new ArrayDeque<EventBean>(2);
        private readonly Deque<EventBean> eventsOldView = new ArrayDeque<EventBean>(2);
        private readonly Deque<MultiKeyArrayOfKeys<EventBean>> eventsNewJoin = new ArrayDeque<MultiKeyArrayOfKeys<EventBean>>(2);
        private readonly Deque<MultiKeyArrayOfKeys<EventBean>> eventsOldJoin = new ArrayDeque<MultiKeyArrayOfKeys<EventBean>>(2);

        public ResultSetProcessorSimpleOutputAllHelperImpl(ResultSetProcessorSimple processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (!processor.HasHavingClause) {
                AddToView(newData, oldData);
                return;
            }

            var eventsPerStream = new EventBean[1];
            if (newData != null && newData.Length > 0) {
                foreach (var theEvent in newData) {
                    eventsPerStream[0] = theEvent;

                    var passesHaving = processor.EvaluateHavingClause(
                        eventsPerStream,
                        true,
                        processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    eventsNewView.Add(theEvent);
                }
            }

            if (oldData != null && oldData.Length > 0) {
                foreach (var theEvent in oldData) {
                    eventsPerStream[0] = theEvent;

                    var passesHaving = processor.EvaluateHavingClause(
                        eventsPerStream,
                        false,
                        processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    eventsOldView.Add(theEvent);
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            if (!processor.HasHavingClause) {
                AddToJoin(newEvents, oldEvents);
                return;
            }

            if (newEvents != null && newEvents.Count > 0) {
                foreach (var theEvent in newEvents) {
                    var passesHaving = processor.EvaluateHavingClause(
                        theEvent.Array,
                        true,
                        processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    eventsNewJoin.Add(theEvent);
                }
            }

            if (oldEvents != null && oldEvents.Count > 0) {
                foreach (var theEvent in oldEvents) {
                    var passesHaving = processor.EvaluateHavingClause(
                        theEvent.Array,
                        false,
                        processor.ExprEvaluatorContext);
                    if (!passesHaving) {
                        continue;
                    }

                    eventsOldJoin.Add(theEvent);
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            var pair = processor.ProcessViewResult(
                EventBeanUtility.ToArrayNullIfEmpty(eventsNewView),
                EventBeanUtility.ToArrayNullIfEmpty(eventsOldView),
                isSynthesize);
            eventsNewView.Clear();
            eventsOldView.Clear();
            return pair;
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            var pair = processor.ProcessJoinResult(
                EventBeanUtility.ToLinkedHashSetNullIfEmpty(eventsNewJoin),
                EventBeanUtility.ToLinkedHashSetNullIfEmpty(eventsOldJoin),
                isSynthesize);
            eventsNewJoin.Clear();
            eventsOldJoin.Clear();
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
            EventBeanUtility.AddToCollection(newData, eventsNewView);
            EventBeanUtility.AddToCollection(oldData, eventsOldView);
        }

        private void AddToJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            EventBeanUtility.AddToCollection(newEvents, eventsNewJoin);
            EventBeanUtility.AddToCollection(oldEvents, eventsOldJoin);
        }
    }
} // end of namespace