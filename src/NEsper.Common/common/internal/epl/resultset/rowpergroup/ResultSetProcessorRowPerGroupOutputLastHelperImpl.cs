///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public class ResultSetProcessorRowPerGroupOutputLastHelperImpl : ResultSetProcessorRowPerGroupOutputLastHelper
    {
        private readonly ResultSetProcessorRowPerGroup processor;
        private readonly IDictionary<object, EventBean[]> groupReps = new LinkedHashMap<object, EventBean[]>();

        private readonly IDictionary<object, EventBean> groupRepsOutputLastUnordRStream =
            new LinkedHashMap<object, EventBean>();

        public ResultSetProcessorRowPerGroupOutputLastHelperImpl(ResultSetProcessorRowPerGroup processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            if (newData != null) {
                foreach (var aNewData in newData) {
                    var eventsPerStream = new EventBean[] { aNewData };
                    var mk = processor.GenerateGroupKeySingle(eventsPerStream, true);

                    // if this is a newly encountered group, generate the remove stream event
                    if (!groupReps.TryPush(mk, eventsPerStream)) {
                        if (processor.IsSelectRStream) {
                            var @event = processor.GenerateOutputBatchedNoSortWMap(
                                false,
                                mk,
                                eventsPerStream,
                                true,
                                isGenerateSynthetic);
                            if (@event != null) {
                                groupRepsOutputLastUnordRStream.Put(mk, @event);
                            }
                        }
                    }

                    processor.AggregationService.ApplyEnter(eventsPerStream, mk, processor.ExprEvaluatorContext);
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    var eventsPerStream = new EventBean[] { anOldData };
                    var mk = processor.GenerateGroupKeySingle(eventsPerStream, true);

                    if (!groupReps.TryPush(mk, eventsPerStream)) {
                        if (processor.IsSelectRStream) {
                            var @event = processor.GenerateOutputBatchedNoSortWMap(
                                false,
                                mk,
                                eventsPerStream,
                                false,
                                isGenerateSynthetic);
                            if (@event != null) {
                                groupRepsOutputLastUnordRStream.Put(mk, @event);
                            }
                        }
                    }

                    processor.AggregationService.ApplyLeave(eventsPerStream, mk, processor.ExprEvaluatorContext);
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldData,
            bool isGenerateSynthetic)
        {
            if (newData != null) {
                foreach (var aNewData in newData) {
                    var mk = processor.GenerateGroupKeySingle(aNewData.Array, true);
                    if (!groupReps.TryPush(mk, aNewData.Array)) {
                        if (processor.IsSelectRStream) {
                            var @event = processor.GenerateOutputBatchedNoSortWMap(
                                true,
                                mk,
                                aNewData.Array,
                                false,
                                isGenerateSynthetic);
                            if (@event != null) {
                                groupRepsOutputLastUnordRStream.Put(mk, @event);
                            }
                        }
                    }

                    processor.AggregationService.ApplyEnter(aNewData.Array, mk, processor.ExprEvaluatorContext);
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    var mk = processor.GenerateGroupKeySingle(anOldData.Array, false);
                    if (!groupReps.TryPush(mk, anOldData.Array)) {
                        if (processor.IsSelectRStream) {
                            var @event = processor.GenerateOutputBatchedNoSortWMap(
                                true,
                                mk,
                                anOldData.Array,
                                false,
                                isGenerateSynthetic);
                            if (@event != null) {
                                groupRepsOutputLastUnordRStream.Put(mk, @event);
                            }
                        }
                    }

                    processor.AggregationService.ApplyLeave(anOldData.Array, mk, processor.ExprEvaluatorContext);
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            return Output(isSynthesize, false);
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            return Output(isSynthesize, true);
        }

        public void Destroy()
        {
            // no action required
        }

        public void Remove(object key)
        {
            groupReps.Remove(key);
        }

        private UniformPair<EventBean[]> Output(
            bool isSynthesize,
            bool join)
        {
            IList<EventBean> newEvents = new List<EventBean>(4);
            processor.GenerateOutputBatchedArrFromIterator(
                join,
                groupReps.GetEnumerator(),
                true,
                isSynthesize,
                newEvents,
                null);
            groupReps.Clear();
            var newEventsArr = newEvents.IsEmpty() ? null : newEvents.ToArray();

            EventBean[] oldEventsArr = null;
            if (!groupRepsOutputLastUnordRStream.IsEmpty()) {
                var oldEvents = groupRepsOutputLastUnordRStream.Values;
                oldEventsArr = oldEvents.ToArray();
                groupRepsOutputLastUnordRStream.Clear();
            }

            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    }
} // end of namespace