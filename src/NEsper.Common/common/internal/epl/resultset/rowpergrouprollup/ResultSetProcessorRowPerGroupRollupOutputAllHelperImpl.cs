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

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl :
        ResultSetProcessorRowPerGroupRollupOutputAllHelper
    {
        private readonly IDictionary<object, EventBean>[] groupRepsOutputLastUnordRStream;
        private readonly IDictionary<object, EventBean[]>[] outputLimitGroupRepsPerLevel;

        private readonly ResultSetProcessorRowPerGroupRollup processor;
        private bool first;

        public ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl(
            ResultSetProcessorRowPerGroupRollup processor,
            int levelCount)
        {
            this.processor = processor;

            outputLimitGroupRepsPerLevel = new LinkedHashMap<object, EventBean[]>[levelCount];
            for (var i = 0; i < levelCount; i++) {
                outputLimitGroupRepsPerLevel[i] = new LinkedHashMap<object, EventBean[]>();
            }

            if (processor.IsSelectRStream) {
                groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>[levelCount];
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
                    eventsPerStream = new[] {aNewData};
                    var groupKeyComplete = processor.GenerateGroupKeySingle(eventsPerStream, true);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (outputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
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
                        processor.GetAgentInstanceContext());
                }
            }

            if (oldData != null) {
                foreach (var anOldData in oldData) {
                    eventsPerStream = new[] {anOldData};
                    var groupKeyComplete = processor.GenerateGroupKeySingle(eventsPerStream, false);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (outputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
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
                        processor.GetAgentInstanceContext());
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
                        if (outputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData) == null) {
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
                        processor.GetAgentInstanceContext());
                }
            }

            if (oldEvents != null) {
                foreach (var oldEvent in oldEvents) {
                    var aOldData = oldEvent.Array;
                    var groupKeyComplete = processor.GenerateGroupKeySingle(aOldData, false);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                        if (outputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aOldData) == null) {
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
                        processor.GetAgentInstanceContext());
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

            var newEventsArr = newEvents.ToArrayOrNull();

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