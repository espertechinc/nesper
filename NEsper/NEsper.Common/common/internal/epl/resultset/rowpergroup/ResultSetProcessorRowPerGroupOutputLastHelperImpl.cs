///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    using DictionaryEventBean = IDictionary<object, EventBean>;
    using DictionaryEventBeanArray = IDictionary<object, EventBean[]>;
    using LinkedEventBean = LinkedHashMap<object, EventBean>;
    using LinkedEventBeanArray = LinkedHashMap<object, EventBean[]>;

    public class ResultSetProcessorRowPerGroupOutputLastHelperImpl : ResultSetProcessorRowPerGroupOutputLastHelper
    {
        private readonly DictionaryEventBeanArray groupReps = new LinkedEventBeanArray();
        private readonly DictionaryEventBean groupRepsOutputLastUnordRStream = new LinkedEventBean();
        protected internal readonly ResultSetProcessorRowPerGroup processor;

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
                    EventBean[] eventsPerStream = {aNewData};
                    var mk = processor.GenerateGroupKeySingle(eventsPerStream, true);

                    // if this is a newly encountered group, generate the remove stream event
                    if (groupReps.Push(mk, eventsPerStream) == null) {
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

                    processor.AggregationService.ApplyEnter(eventsPerStream, mk, processor.GetAgentInstanceContext());
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    EventBean[] eventsPerStream = {anOldData};
                    var mk = processor.GenerateGroupKeySingle(eventsPerStream, true);

                    if (groupReps.Push(mk, eventsPerStream) == null) {
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

                    processor.AggregationService.ApplyLeave(eventsPerStream, mk, processor.GetAgentInstanceContext());
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKey<EventBean>> newData,
            ISet<MultiKey<EventBean>> oldData,
            bool isGenerateSynthetic)
        {
            if (newData != null) {
                foreach (var aNewData in newData) {
                    var mk = processor.GenerateGroupKeySingle(aNewData.Array, true);
                    if (groupReps.Push(mk, aNewData.Array) == null) {
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

                    processor.AggregationService.ApplyEnter(aNewData.Array, mk, processor.GetAgentInstanceContext());
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    var mk = processor.GenerateGroupKeySingle(anOldData.Array, false);
                    if (groupReps.Push(mk, anOldData.Array) == null) {
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

                    processor.AggregationService.ApplyLeave(anOldData.Array, mk, processor.GetAgentInstanceContext());
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
            }

            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    }
} // end of namespace