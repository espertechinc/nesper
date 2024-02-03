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


namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    public class ResultSetProcessorAggregateGroupedOutputAllHelperImpl :
        ResultSetProcessorAggregateGroupedOutputAllHelper
    {
        private readonly ResultSetProcessorAggregateGrouped processor;

        private readonly IList<EventBean> eventsOld = new List<EventBean>(2);
        private readonly IList<EventBean> eventsNew = new List<EventBean>(2);
        private readonly IDictionary<object, EventBean[]> repsPerGroup = new LinkedHashMap<object, EventBean[]>();
        private readonly ISet<object> lastSeenKeys = new HashSet<object>();

        public ResultSetProcessorAggregateGroupedOutputAllHelperImpl(ResultSetProcessorAggregateGrouped processor)
        {
            this.processor = processor;
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            var newDataMultiKey = processor.GenerateGroupKeyArrayView(newData, true);
            var oldDataMultiKey = processor.GenerateGroupKeyArrayView(oldData, false);
            ISet<object> keysSeenRemoved = new HashSet<object>();

            var eventsPerStreamOneStream = new EventBean[1];
            if (newData != null) {
                // apply new data to aggregates
                var count = 0;
                foreach (var aNewData in newData) {
                    var eventsPerStream = new EventBean[] { aNewData };
                    var mk = newDataMultiKey[count];
                    repsPerGroup.Put(mk, eventsPerStream);
                    lastSeenKeys.Add(mk);
                    processor.AggregationService.ApplyEnter(eventsPerStream, mk, processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                var count = 0;
                foreach (var anOldData in oldData) {
                    var mk = oldDataMultiKey[count];
                    lastSeenKeys.Add(mk);
                    keysSeenRemoved.Add(mk);
                    eventsPerStreamOneStream[0] = anOldData;
                    processor.AggregationService.ApplyLeave(
                        eventsPerStreamOneStream,
                        oldDataMultiKey[count],
                        processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (processor.IsSelectRStream) {
                processor.GenerateOutputBatchedViewUnkeyed(
                    oldData,
                    oldDataMultiKey,
                    false,
                    isGenerateSynthetic,
                    eventsOld,
                    null,
                    eventsPerStreamOneStream);
            }

            processor.GenerateOutputBatchedViewUnkeyed(
                newData,
                newDataMultiKey,
                true,
                isGenerateSynthetic,
                eventsNew,
                null,
                eventsPerStreamOneStream);

            foreach (var keySeen in keysSeenRemoved) {
                var newEvent = processor.GenerateOutputBatchedSingle(
                    keySeen,
                    repsPerGroup.Get(keySeen),
                    true,
                    isGenerateSynthetic);
                if (newEvent != null) {
                    eventsNew.Add(newEvent);
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldData,
            bool isGenerateSynthetic)
        {
            var newDataMultiKey = processor.GenerateGroupKeyArrayJoin(newData, true);
            var oldDataMultiKey = processor.GenerateGroupKeyArrayJoin(oldData, false);
            ISet<object> keysSeenRemoved = new HashSet<object>();

            if (newData != null) {
                // apply new data to aggregates
                var count = 0;
                foreach (var aNewData in newData) {
                    var mk = newDataMultiKey[count];
                    repsPerGroup.Put(mk, aNewData.Array);
                    lastSeenKeys.Add(mk);
                    processor.AggregationService.ApplyEnter(aNewData.Array, mk, processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                var count = 0;
                foreach (var anOldData in oldData) {
                    var mk = oldDataMultiKey[count];
                    lastSeenKeys.Add(mk);
                    keysSeenRemoved.Add(mk);
                    processor.AggregationService.ApplyLeave(
                        anOldData.Array,
                        oldDataMultiKey[count],
                        processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (processor.IsSelectRStream) {
                processor.GenerateOutputBatchedJoinUnkeyed(
                    oldData,
                    oldDataMultiKey,
                    false,
                    isGenerateSynthetic,
                    eventsOld,
                    null);
            }

            processor.GenerateOutputBatchedJoinUnkeyed(
                newData,
                newDataMultiKey,
                false,
                isGenerateSynthetic,
                eventsNew,
                null);

            foreach (var keySeen in keysSeenRemoved) {
                var newEvent = processor.GenerateOutputBatchedSingle(
                    keySeen,
                    repsPerGroup.Get(keySeen),
                    true,
                    isGenerateSynthetic);
                if (newEvent != null) {
                    eventsNew.Add(newEvent);
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            return Output(isSynthesize);
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            return Output(isSynthesize);
        }

        public void Remove(object key)
        {
            repsPerGroup.Remove(key);
        }

        public void Destroy()
        {
            // no action required
        }

        private UniformPair<EventBean[]> Output(bool isSynthesize)
        {
            // generate remaining key events
            foreach (var entry in repsPerGroup) {
                if (lastSeenKeys.Contains(entry.Key)) {
                    continue;
                }

                var newEvent = processor.GenerateOutputBatchedSingle(entry.Key, entry.Value, true, isSynthesize);
                if (newEvent != null) {
                    eventsNew.Add(newEvent);
                }
            }

            lastSeenKeys.Clear();

            var newEventsArr = eventsNew.ToArrayOrNull();
            EventBean[] oldEventsArr = null;
            if (processor.IsSelectRStream) {
                oldEventsArr = eventsOld.ToArrayOrNull();
            }

            eventsNew.Clear();
            eventsOld.Clear();
            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    }
} // end of namespace