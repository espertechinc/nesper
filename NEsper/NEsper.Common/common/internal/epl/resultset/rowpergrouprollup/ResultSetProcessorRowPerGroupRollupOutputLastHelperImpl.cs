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
    using DictionaryEventBean = IDictionary<object, EventBean>;
    using DictionaryEventBeanArray = IDictionary<object, EventBean[]>;

    public class ResultSetProcessorRowPerGroupRollupOutputLastHelperImpl :
        ResultSetProcessorRowPerGroupRollupOutputLastHelper
    {
        private readonly DictionaryEventBean[] groupRepsOutputLastUnordRStream;
        private readonly DictionaryEventBeanArray[] outputLimitGroupRepsPerLevel;
        private readonly ResultSetProcessorRowPerGroupRollup processor;

        public ResultSetProcessorRowPerGroupRollupOutputLastHelperImpl(
            ResultSetProcessorRowPerGroupRollup processor,
            int levelCount)
        {
            this.processor = processor;

            outputLimitGroupRepsPerLevel = new DictionaryEventBeanArray[levelCount];
            for (var i = 0; i < levelCount; i++) {
                outputLimitGroupRepsPerLevel[i] = new LinkedHashMap<object, EventBean[]>();
            }

            if (processor.IsSelectRStream) {
                groupRepsOutputLastUnordRStream = new DictionaryEventBean[levelCount];
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

                        outputLimitGroupRepsPerLevel[level.LevelNumber].Put(groupKey, eventsPerStream);
                        if (processor.IsSelectRStream &&
                            !groupRepsOutputLastUnordRStream[level.LevelNumber].ContainsKey(groupKey)) {
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

                        outputLimitGroupRepsPerLevel[level.LevelNumber].Put(groupKey, eventsPerStream);
                        if (processor.IsSelectRStream &&
                            !groupRepsOutputLastUnordRStream[level.LevelNumber].ContainsKey(groupKey)) {
                            processor.GenerateOutputBatchedMapUnsorted(
                                false,
                                groupKey,
                                level,
                                eventsPerStream,
                                false,
                                isGenerateSynthetic,
                                groupRepsOutputLastUnordRStream[level.LevelNumber]);
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
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isGenerateSynthetic)
        {
            // apply to aggregates
            var groupKeysPerLevel = new object[processor.GroupByRollupDesc.Levels.Length];
            if (newEvents != null) {
                foreach (var newEvent in newEvents) {
                    var aNewData = newEvent.Array;
                    var groupKeyComplete = processor.GenerateGroupKeySingle(aNewData, true);
                    foreach (var level in processor.GroupByRollupDesc.Levels) {
                        var groupKey = level.ComputeSubkey(groupKeyComplete);
                        groupKeysPerLevel[level.LevelNumber] = groupKey;

                        outputLimitGroupRepsPerLevel[level.LevelNumber].Put(groupKey, aNewData);
                        if (processor.IsSelectRStream &&
                            !groupRepsOutputLastUnordRStream[level.LevelNumber].ContainsKey(groupKey)) {
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

                        outputLimitGroupRepsPerLevel[level.LevelNumber].Put(groupKey, aOldData);
                        if (processor.IsSelectRStream &&
                            !groupRepsOutputLastUnordRStream[level.LevelNumber].ContainsKey(groupKey)) {
                            processor.GenerateOutputBatchedMapUnsorted(
                                false,
                                groupKey,
                                level,
                                aOldData,
                                false,
                                isGenerateSynthetic,
                                groupRepsOutputLastUnordRStream[level.LevelNumber]);
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
            foreach (var outputLimitGroupRepsPerLevelItem in outputLimitGroupRepsPerLevel) {
                outputLimitGroupRepsPerLevelItem.Clear();
            }

            EventBean[] oldEventsArr = null;
            if (groupRepsOutputLastUnordRStream != null) {
                IList<EventBean> oldEventList = new List<EventBean>(4);
                foreach (var entry in groupRepsOutputLastUnordRStream) {
                    oldEventList.AddAll(entry.Values);
                }

                if (!oldEventList.IsEmpty()) {
                    oldEventsArr = oldEventList.ToArray();
                    foreach (var groupRepsOutputLastUnordRStreamItem in groupRepsOutputLastUnordRStream) {
                        groupRepsOutputLastUnordRStreamItem.Clear();
                    }
                }
            }

            if (newEventsArr == null && oldEventsArr == null) {
                return null;
            }

            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    }
} // end of namespace