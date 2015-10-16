///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowPerGroupRollupOutputAllHelper
    {
        private readonly ResultSetProcessorRowPerGroupRollup _processor;
	    private readonly IDictionary<object, EventBean>[] _groupRepsOutputLastUnordRStream;
	    private bool _first;

	    public ResultSetProcessorRowPerGroupRollupOutputAllHelper(ResultSetProcessorRowPerGroupRollup processor, int levelCount)
        {
	        _processor = processor;
            _groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>[levelCount];
	        for (var i = 0; i < levelCount; i++) {
	            _groupRepsOutputLastUnordRStream[i] = new LinkedHashMap<object, EventBean>();
	        }
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic)
        {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, false);

	        // apply to aggregates
            var groupKeysPerLevel = new object[_processor.Prototype.GroupByRollupDesc.Levels.Length];
	        EventBean[] eventsPerStream;
	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                eventsPerStream = new EventBean[] {aNewData};
	                var groupKeyComplete = _processor.GenerateGroupKey(eventsPerStream, true);
	                foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels) {
	                    var groupKey = level.ComputeSubkey(groupKeyComplete);
	                    groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    if (_processor.OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                            if (_processor.Prototype.IsSelectRStream)
                            {
	                            _processor.GenerateOutputBatchedMapUnsorted(false, groupKey, level, eventsPerStream, true, isGenerateSynthetic, _groupRepsOutputLastUnordRStream[level.LevelNumber]);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                eventsPerStream = new EventBean[] {anOldData};
	                var groupKeyComplete = _processor.GenerateGroupKey(eventsPerStream, false);
                    foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels)
                    {
	                    var groupKey = level.ComputeSubkey(groupKeyComplete);
	                    groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    if (_processor.OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                            if (_processor.Prototype.IsSelectRStream)
                            {
	                            _processor.GenerateOutputBatchedMapUnsorted(true, groupKey, level, eventsPerStream, false, isGenerateSynthetic, _groupRepsOutputLastUnordRStream[level.LevelNumber]);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _processor.AgentInstanceContext);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, true);

	        // apply to aggregates
            var groupKeysPerLevel = new object[_processor.Prototype.GroupByRollupDesc.Levels.Length];
	        if (newEvents != null) {
	            foreach (var newEvent in newEvents) {
	                var aNewData = newEvent.Array;
	                var groupKeyComplete = _processor.GenerateGroupKey(aNewData, true);
                    foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels)
                    {
	                    var groupKey = level.ComputeSubkey(groupKeyComplete);
	                    groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    if (_processor.OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData) == null) {
                            if (_processor.Prototype.IsSelectRStream)
                            {
	                            _processor.GenerateOutputBatchedMapUnsorted(false, groupKey, level, aNewData, true, isGenerateSynthetic, _groupRepsOutputLastUnordRStream[level.LevelNumber]);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(aNewData, groupKeysPerLevel, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldEvents != null) {
	            foreach (var oldEvent in oldEvents) {
	                var aOldData = oldEvent.Array;
	                var groupKeyComplete = _processor.GenerateGroupKey(aOldData, false);
                    foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels)
                    {
	                    var groupKey = level.ComputeSubkey(groupKeyComplete);
	                    groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    if (_processor.OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aOldData) == null) {
                            if (_processor.Prototype.IsSelectRStream)
                            {
	                            _processor.GenerateOutputBatchedMapUnsorted(true, groupKey, level, aOldData, false, isGenerateSynthetic, _groupRepsOutputLastUnordRStream[level.LevelNumber]);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyLeave(aOldData, groupKeysPerLevel, _processor.AgentInstanceContext);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        GenerateRemoveStreamJustOnce(isSynthesize, false);
	        return Output(isSynthesize, false);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        GenerateRemoveStreamJustOnce(isSynthesize, true);
	        return Output(isSynthesize, true);
	    }

	    private UniformPair<EventBean[]> Output(bool isSynthesize, bool isJoin) {

	        IList<EventBean> newEvents = new List<EventBean>(4);
            foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels)
            {
	            IDictionary<object, EventBean[]> groupGenerators = _processor.OutputLimitGroupRepsPerLevel[level.LevelNumber];
	            foreach (KeyValuePair<object, EventBean[]> entry in groupGenerators) {
	                _processor.GenerateOutputBatched(isJoin, entry.Key, level, entry.Value, true, isSynthesize, newEvents, null);
	            }
	        }
	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();

	        EventBean[] oldEventsArr = null;
            if (_processor.Prototype.IsSelectRStream)
            {
	            IList<EventBean> oldEventList = new List<EventBean>(4);
	            foreach (var entry in _groupRepsOutputLastUnordRStream) {
	                oldEventList.AddAll(entry.Values);
	                entry.Clear();
	            }
	            if (!oldEventList.IsEmpty()) {
	                oldEventsArr = oldEventList.ToArray();
	            }
	        }

	        _first = true;

	        if (newEventsArr == null && oldEventsArr == null) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private void GenerateRemoveStreamJustOnce(bool isSynthesize, bool join) {
            if (_first && _processor.Prototype.IsSelectRStream)
            {
                foreach (var level in _processor.Prototype.GroupByRollupDesc.Levels)
                {
	                foreach (var groupRep in _processor.OutputLimitGroupRepsPerLevel[level.LevelNumber])
                    {
	                    var groupKeyPartial = _processor.GenerateGroupKey(groupRep.Value, false);
	                    var groupKey = level.ComputeSubkey(groupKeyPartial);
	                    _processor.GenerateOutputBatchedMapUnsorted(join, groupKey, level, groupRep.Value, false, isSynthesize, _groupRepsOutputLastUnordRStream[level.LevelNumber]);
	                }
	            }
	        }
	        _first = false;
	    }
	}
} // end of namespace
