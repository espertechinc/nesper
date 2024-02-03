///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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


namespace com.espertech.esper.common.@internal.epl.resultset.agggrouped
{
    public class ResultSetProcessorAggregateGroupedOutputLastHelperImpl :
        ResultSetProcessorAggregateGroupedOutputLastHelper
    {
        private readonly ResultSetProcessorAggregateGrouped processor;

        private IDictionary<object, EventBean> outputLastUnordGroupNew;
        private IDictionary<object, EventBean> outputLastUnordGroupOld;

        public ResultSetProcessorAggregateGroupedOutputLastHelperImpl(ResultSetProcessorAggregateGrouped processor)
        {
            this.processor = processor;
            outputLastUnordGroupNew = new LinkedHashMap<object, EventBean>();
            outputLastUnordGroupOld = new LinkedHashMap<object, EventBean>();
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            var newDataMultiKey = processor.GenerateGroupKeyArrayView(newData, true);
            var oldDataMultiKey = processor.GenerateGroupKeyArrayView(oldData, false);
            var eventsPerStream = new EventBean[1];

            if (newData != null) {
                // apply new data to aggregates
                var count = 0;
                foreach (var aNewData in newData) {
                    var mk = newDataMultiKey[count];
                    eventsPerStream[0] = aNewData;
                    processor.AggregationService.ApplyEnter(eventsPerStream, mk, processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                var count = 0;
                foreach (var anOldData in oldData) {
                    eventsPerStream[0] = anOldData;
                    processor.AggregationService.ApplyLeave(
                        eventsPerStream,
                        oldDataMultiKey[count],
                        processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (processor.IsSelectRStream) {
                processor.GenerateOutputBatchedViewPerKey(
                    oldData,
                    oldDataMultiKey,
                    false,
                    isGenerateSynthetic,
                    outputLastUnordGroupOld,
                    null,
                    eventsPerStream);
            }

            processor.GenerateOutputBatchedViewPerKey(
                newData,
                newDataMultiKey,
                false,
                isGenerateSynthetic,
                outputLastUnordGroupNew,
                null,
                eventsPerStream);
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newData,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldData,
            bool isGenerateSynthetic)
        {
            var newDataMultiKey = processor.GenerateGroupKeyArrayJoin(newData, true);
            var oldDataMultiKey = processor.GenerateGroupKeyArrayJoin(oldData, false);

            if (newData != null) {
                // apply new data to aggregates
                var count = 0;
                foreach (var aNewData in newData) {
                    var mk = newDataMultiKey[count];
                    processor.AggregationService.ApplyEnter(aNewData.Array, mk, processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (oldData != null) {
                // apply old data to aggregates
                var count = 0;
                foreach (var anOldData in oldData) {
                    processor.AggregationService.ApplyLeave(
                        anOldData.Array,
                        oldDataMultiKey[count],
                        processor.ExprEvaluatorContext);
                    count++;
                }
            }

            if (processor.IsSelectRStream) {
                processor.GenerateOutputBatchedJoinPerKey(
                    oldData,
                    oldDataMultiKey,
                    false,
                    isGenerateSynthetic,
                    outputLastUnordGroupOld,
                    null);
            }

            processor.GenerateOutputBatchedJoinPerKey(
                newData,
                newDataMultiKey,
                false,
                isGenerateSynthetic,
                outputLastUnordGroupNew,
                null);
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            return ContinueOutputLimitedLastNonBuffered();
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            return ContinueOutputLimitedLastNonBuffered();
        }

        public void Remove(object key)
        {
            // no action required
        }

        public void Destroy()
        {
            // no action required
        }

        private UniformPair<EventBean[]> ContinueOutputLimitedLastNonBuffered()
        {
            EventBean[] newEventsArr =
                outputLastUnordGroupNew.IsEmpty() ? null : outputLastUnordGroupNew.Values.ToArray();
            EventBean[] oldEventsArr = null;
            if (processor.IsSelectRStream) {
                oldEventsArr = outputLastUnordGroupOld.IsEmpty() ? null : outputLastUnordGroupOld.Values.ToArray();
            }

            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            outputLastUnordGroupNew.Clear();
            outputLastUnordGroupOld.Clear();
            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    }
} // end of namespace