///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ResultSetProcessorSimpleOutputLastHelperImpl : ResultSetProcessorSimpleOutputLastHelper
    {
        private readonly ResultSetProcessorSimple processor;

        private EventBean outputLastIStreamBufView;
        private EventBean outputLastRStreamBufView;
        private MultiKeyArrayOfKeys<EventBean> outputLastIStreamBufJoin;
        private MultiKeyArrayOfKeys<EventBean> outputLastRStreamBufJoin;

        public ResultSetProcessorSimpleOutputLastHelperImpl(ResultSetProcessorSimple processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (!processor.HasHavingClause) {
                if (newData != null && newData.Length > 0) {
                    outputLastIStreamBufView = newData[^1];
                }

                if (oldData != null && oldData.Length > 0) {
                    outputLastRStreamBufView = oldData[^1];
                }
            }
            else {
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

                        outputLastIStreamBufView = theEvent;
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

                        outputLastRStreamBufView = theEvent;
                    }
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            if (!processor.HasHavingClause) {
                if (newEvents != null && !newEvents.IsEmpty()) {
                    outputLastIStreamBufJoin = EventBeanUtility.GetLastInSet(newEvents);
                }

                if (oldEvents != null && !oldEvents.IsEmpty()) {
                    outputLastRStreamBufJoin = EventBeanUtility.GetLastInSet(oldEvents);
                }
            }
            else {
                if (newEvents != null && newEvents.Count > 0) {
                    foreach (var theEvent in newEvents) {
                        var passesHaving = processor.EvaluateHavingClause(
                            theEvent.Array,
                            true,
                            processor.ExprEvaluatorContext);
                        if (!passesHaving) {
                            continue;
                        }

                        outputLastIStreamBufJoin = theEvent;
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

                        outputLastRStreamBufJoin = theEvent;
                    }
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            if (outputLastIStreamBufView == null && outputLastRStreamBufView == null) {
                return null;
            }

            var pair = processor.ProcessViewResult(
                EventBeanUtility.ToArrayIfNotNull(outputLastIStreamBufView),
                EventBeanUtility.ToArrayIfNotNull(outputLastRStreamBufView),
                isSynthesize);
            outputLastIStreamBufView = null;
            outputLastRStreamBufView = null;
            return pair;
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            if (outputLastIStreamBufJoin == null && outputLastRStreamBufJoin == null) {
                return null;
            }

            var pair = processor.ProcessJoinResult(
                EventBeanUtility.ToSingletonSetIfNotNull(outputLastIStreamBufJoin),
                EventBeanUtility.ToSingletonSetIfNotNull(outputLastRStreamBufJoin),
                isSynthesize);
            outputLastIStreamBufJoin = null;
            outputLastRStreamBufJoin = null;
            return pair;
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace