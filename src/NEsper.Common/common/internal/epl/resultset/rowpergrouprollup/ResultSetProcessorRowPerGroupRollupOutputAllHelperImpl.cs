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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl :
        ResultSetProcessorRowPerGroupRollupOutputAllHelper
    {
        private readonly ResultSetProcessorRowPerGroupRollup processor;
        private readonly IDictionary<object, EventBean[]>[] outputLimitGroupRepsPerLevel;
        private readonly IDictionary<object, EventBean>[] groupRepsOutputLastUnordRStream;
        private bool first;

        public ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl(
            ResultSetProcessorRowPerGroupRollup processor,
            int levelCount)
        {
            this.processor = processor;

            outputLimitGroupRepsPerLevel = (LinkedHashMap<object, EventBean[]>[])new LinkedHashMap<object, EventBean[]>[levelCount];
            for (var i = 0; i < levelCount; i++) {
                outputLimitGroupRepsPerLevel[i] = new LinkedHashMap<object, EventBean[]>();
            }

            if (processor.IsSelectRStream) {
                groupRepsOutputLastUnordRStream = new IDictionary<object, EventBean>[levelCount]; 
                for (var i = 0; i < levelCount; i++) {
                    groupRepsOutputLastUnordRStream[i] = new LinkedHashMap<object, EventBean>();
                }
            }
            else {
                groupRepsOutputLastUnordRStream = null;
            }
        }

        public void ProcessView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic)
        {
            GenerateRemoveStreamJustOnce(isGenerateSynthetic, false);

            // apply to aggregates
            var groupKeysPerLevel = new object[processor.GroupByRollupDesc.Levels.Length];
            EventBean[] eventsPerStream;
            if (newData != null) {
                foreach (var aNewData in newData) {
                    eventsPerStream = new EventBean[] { aNewData };
                    var groupKeyComplete = processor.GenerateGroupKeySingle(eventsPerStream, true);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (!outputLimitGroupRepsPerLevel[level.LevelNumber].TryPush(groupKey, eventsPerStream)) {
                            if (processor.IsSelectRStream) {
                                processor.GenerateOutputBatchedMapUnsorted(
                                    false,
                                    groupKey,
                                    level,
                                    eventsPerStream,
                                    true,
                                    isGenerateSynthetic,
                                    groupRepsOutputLastUnordRStream[level.LevelNumber]);
                            }
                        }
                    }

                    processor.AggregationService.ApplyEnter(
                        eventsPerStream,
                        groupKeysPerLevel,
                        processor.ExprEvaluatorContext);
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    eventsPerStream = new EventBean[] { anOldData };
                    var groupKeyComplete = processor.GenerateGroupKeySingle(eventsPerStream, false);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (!outputLimitGroupRepsPerLevel[level.LevelNumber].TryPush(groupKey, eventsPerStream)) {
                            if (processor.IsSelectRStream) {
                                processor.GenerateOutputBatchedMapUnsorted(
                                    true,
                                    groupKey,
                                    level,
                                    eventsPerStream,
                                    false,
                                    isGenerateSynthetic,
                                    groupRepsOutputLastUnordRStream[level.LevelNumber]);
                            }
                        }
                    }

                    processor.AggregationService.ApplyLeave(
                        eventsPerStream,
                        groupKeysPerLevel,
                        processor.ExprEvaluatorContext);
                }
            }
        }

        public void ProcessJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            GenerateRemoveStreamJustOnce(isGenerateSynthetic, true);

            // apply to aggregates
            var groupKeysPerLevel = new object[processor.GroupByRollupDesc.Levels.Length];
            if (newEvents != null) {
                foreach (var newEvent in newEvents) {
                    var aNewData = newEvent.Array;
                    var groupKeyComplete = processor.GenerateGroupKeySingle(aNewData, true);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (!outputLimitGroupRepsPerLevel[level.LevelNumber].TryPush(groupKey, aNewData)) {
                            if (processor.IsSelectRStream) {
                                processor.GenerateOutputBatchedMapUnsorted(
                                    false,
                                    groupKey,
                                    level,
                                    aNewData,
                                    true,
                                    isGenerateSynthetic,
                                    groupRepsOutputLastUnordRStream[level.LevelNumber]);
                            }
                        }
                    }

                    processor.AggregationService.ApplyEnter(
                        aNewData,
                        groupKeysPerLevel,
                        processor.ExprEvaluatorContext);
                }
            }

            if (oldEvents != null) {
                foreach (var oldEvent in oldEvents) {
                    var aOldData = oldEvent.Array;
                    var groupKeyComplete = processor.GenerateGroupKeySingle(aOldData, false);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (!outputLimitGroupRepsPerLevel[level.LevelNumber].TryPush(groupKey, aOldData)) {
                            if (processor.IsSelectRStream) {
                                processor.GenerateOutputBatchedMapUnsorted(
                                    true,
                                    groupKey,
                                    level,
                                    aOldData,
                                    false,
                                    isGenerateSynthetic,
                                    groupRepsOutputLastUnordRStream[level.LevelNumber]);
                            }
                        }
                    }

                    processor.AggregationService.ApplyLeave(
                        aOldData,
                        groupKeysPerLevel,
                        processor.ExprEvaluatorContext);
                }
            }
        }

        public UniformPair<EventBean[]> OutputView(bool isSynthesize)
        {
            GenerateRemoveStreamJustOnce(isSynthesize, false);
            return Output(isSynthesize, false);
        }

        public UniformPair<EventBean[]> OutputJoin(bool isSynthesize)
        {
            GenerateRemoveStreamJustOnce(isSynthesize, true);
            return Output(isSynthesize, true);
        }

        public void Destroy()
        {
            // no action required
        }

        private UniformPair<EventBean[]> Output(
            bool isSynthesize,
            bool isJoin)
        {
            IList<EventBean> newEvents = new List<EventBean>(4);
            foreach (var level in processor.GroupByRollupDesc.Levels) {
                var groupGenerators = outputLimitGroupRepsPerLevel[level.LevelNumber];
                foreach (var entry in groupGenerators) {
                    processor.GenerateOutputBatched(entry.Key, level, entry.Value, true, isSynthesize, newEvents, null);
                }
            }

            var newEventsArr = newEvents.IsEmpty() ? null : newEvents.ToArray();

            EventBean[] oldEventsArr = null;
            if (processor.IsSelectRStream) {
                IList<EventBean> oldEventList = new List<EventBean>(4);
                foreach (var entry in groupRepsOutputLastUnordRStream) {
                    oldEventList.AddAll(entry.Values);
                    entry.Clear();
                }

                if (!oldEventList.IsEmpty()) {
                    oldEventsArr = oldEventList.ToArray();
                }
            }

            first = true;

            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }

        private void GenerateRemoveStreamJustOnce(
            bool isSynthesize,
            bool join)
        {
            if (first && processor.IsSelectRStream) {
                foreach (var level in processor.GroupByRollupDesc.Levels) {
                    foreach (var groupRep in outputLimitGroupRepsPerLevel[level.LevelNumber]) {
                        var groupKeyPartial = processor.GenerateGroupKeySingle(groupRep.Value, false);
                        var groupKey = level.ComputeSubkey(groupKeyPartial);
                        processor.GenerateOutputBatchedMapUnsorted(
                            join,
                            groupKey,
                            level,
                            groupRep.Value,
                            false,
                            isSynthesize,
                            groupRepsOutputLastUnordRStream[level.LevelNumber]);
                    }
                }
            }

            first = false;
        }
    }
} // end of namespace