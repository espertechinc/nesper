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
    public class ResultSetProcessorSimpleOutputLastHelperImpl : ResultSetProcessorSimpleOutputLastHelper
    {
        private readonly ResultSetProcessorSimple _processor;
        private MultiKeyArrayOfKeys<EventBean> _outputLastIStreamBufJoin;

        private EventBean _outputLastIStreamBufView;
        private MultiKeyArrayOfKeys<EventBean> _outputLastRStreamBufJoin;
        private EventBean _outputLastRStreamBufView;

        public ResultSetProcessorSimpleOutputLastHelperImpl(ResultSetProcessorSimple processor)
        {
            _processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (!_processor.HasHavingClause) {
                if (newData != null && newData.Length > 0) {
                    _outputLastIStreamBufView = newData[newData.Length - 1];
                }

                if (oldData != null && oldData.Length > 0) {
                    _outputLastRStreamBufView = oldData[oldData.Length - 1];
                }
            }
            else {
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

                        _outputLastIStreamBufView = theEvent;
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

                        _outputLastRStreamBufView = theEvent;
                    }
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents)
        {
            if (!_processor.HasHavingClause) {
                if (newEvents != null && !newEvents.IsEmpty()) {
                    _outputLastIStreamBufJoin = EventBeanUtility.GetLastInSet(newEvents);
                }

                if (oldEvents != null && !oldEvents.IsEmpty()) {
                    _outputLastRStreamBufJoin = EventBeanUtility.GetLastInSet(oldEvents);
                }
            }
            else {
                if (newEvents != null && newEvents.Count > 0) {
                    foreach (var theEvent in newEvents) {
                        var passesHaving = _processor.EvaluateHavingClause(
                            theEvent.Array,
                            true,
                            _processor.ExprEvaluatorContext);
                        if (!passesHaving) {
                            continue;
                        }

                        _outputLastIStreamBufJoin = theEvent;
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

                        _outputLastRStreamBufJoin = theEvent;
                    }
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            if (_outputLastIStreamBufView == null && _outputLastRStreamBufView == null) {
                return null;
            }

            var pair = _processor.ProcessViewResult(
                EventBeanUtility.ToArrayIfNotNull(_outputLastIStreamBufView),
                EventBeanUtility.ToArrayIfNotNull(_outputLastRStreamBufView),
                isSynthesize);
            _outputLastIStreamBufView = null;
            _outputLastRStreamBufView = null;
            return pair;
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            if (_outputLastIStreamBufJoin == null && _outputLastRStreamBufJoin == null) {
                return null;
            }

            var pair = _processor.ProcessJoinResult(
                EventBeanUtility.ToSingletonSetIfNotNull(_outputLastIStreamBufJoin),
                EventBeanUtility.ToSingletonSetIfNotNull(_outputLastRStreamBufJoin),
                isSynthesize);
            _outputLastIStreamBufJoin = null;
            _outputLastRStreamBufJoin = null;
            return pair;
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace