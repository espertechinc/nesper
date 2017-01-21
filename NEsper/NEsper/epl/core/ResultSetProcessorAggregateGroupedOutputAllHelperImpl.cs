///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorAggregateGroupedOutputAllHelperImpl : ResultSetProcessorAggregateGroupedOutputAllHelper
    {
	    private readonly ResultSetProcessorAggregateGrouped _processor;

	    private readonly IList<EventBean> _eventsOld = new List<EventBean>(2);
	    private readonly IList<EventBean> _eventsNew = new List<EventBean>(2);
	    private readonly IDictionary<object, EventBean[]> _repsPerGroup = new LinkedHashMap<object, EventBean[]>();
	    private readonly ISet<object> _lastSeenKeys = new HashSet<object>();

	    public ResultSetProcessorAggregateGroupedOutputAllHelperImpl(ResultSetProcessorAggregateGrouped processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        object[] newDataMultiKey = _processor.GenerateGroupKeys(newData, true);
	        object[] oldDataMultiKey = _processor.GenerateGroupKeys(oldData, false);
	        ISet<object> keysSeenRemoved = new HashSet<object>();

	        if (newData != null)
	        {
	            // apply new data to aggregates
	            int count = 0;
	            foreach (EventBean aNewData in newData)
	            {
	                EventBean[] eventsPerStream = new EventBean[] {aNewData};
	                object mk = newDataMultiKey[count];
	                _repsPerGroup.Put(mk, eventsPerStream);
	                _lastSeenKeys.Add(mk);
	                _processor.EventsPerStreamOneStream[0] = aNewData;
	                _processor.AggregationService.ApplyEnter(eventsPerStream, mk, _processor.AgentInstanceContext);
	                count++;
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            int count = 0;
	            foreach (EventBean anOldData in oldData)
	            {
	                object mk = oldDataMultiKey[count];
	                _lastSeenKeys.Add(mk);
	                keysSeenRemoved.Add(mk);
	                _processor.EventsPerStreamOneStream[0] = anOldData;
	                _processor.AggregationService.ApplyLeave(_processor.EventsPerStreamOneStream, oldDataMultiKey[count], _processor.AgentInstanceContext);
	                count++;
	            }
	        }

	        if (_processor.Prototype.IsSelectRStream) {
	            _processor.GenerateOutputBatchedViewUnkeyed(oldData, oldDataMultiKey, false, isGenerateSynthetic, _eventsOld, null);
	        }
	        _processor.GenerateOutputBatchedViewUnkeyed(newData, newDataMultiKey, true, isGenerateSynthetic, _eventsNew, null);

	        foreach (object keySeen in keysSeenRemoved) {
	            EventBean newEvent = _processor.GenerateOutputBatchedSingle(keySeen, _repsPerGroup.Get(keySeen), true, isGenerateSynthetic);
	            if (newEvent != null) {
	                _eventsNew.Add(newEvent);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        object[] newDataMultiKey = _processor.GenerateGroupKeys(newData, true);
	        object[] oldDataMultiKey = _processor.GenerateGroupKeys(oldData, false);
	        ISet<object> keysSeenRemoved = new HashSet<object>();

	        if (newData != null) {
	            // apply new data to aggregates
	            int count = 0;
	            foreach (MultiKey<EventBean> aNewData in newData)
	            {
	                object mk = newDataMultiKey[count];
	                _repsPerGroup.Put(mk, aNewData.Array);
	                _lastSeenKeys.Add(mk);
	                _processor.AggregationService.ApplyEnter(aNewData.Array, mk, _processor.AgentInstanceContext);
	                count++;
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            int count = 0;
	            foreach (MultiKey<EventBean> anOldData in oldData)
	            {
	                object mk = oldDataMultiKey[count];
	                _lastSeenKeys.Add(mk);
	                keysSeenRemoved.Add(mk);
	                _processor.AggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _processor.AgentInstanceContext);
	                count++;
	            }
	        }

	        if (_processor.Prototype.IsSelectRStream) {
	            _processor.GenerateOutputBatchedJoinUnkeyed(oldData, oldDataMultiKey, false, isGenerateSynthetic, _eventsOld, null);
	        }
	        _processor.GenerateOutputBatchedJoinUnkeyed(newData, newDataMultiKey, false, isGenerateSynthetic, _eventsNew, null);

	        foreach (object keySeen in keysSeenRemoved) {
	            EventBean newEvent = _processor.GenerateOutputBatchedSingle(keySeen, _repsPerGroup.Get(keySeen), true, isGenerateSynthetic);
	            if (newEvent != null) {
	                _eventsNew.Add(newEvent);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return Output(isSynthesize);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return Output(isSynthesize);
	    }

	    public void Remove(object key) {
	        _repsPerGroup.Remove(key);
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private UniformPair<EventBean[]> Output(bool isSynthesize) {
	        // generate remaining key events
	        foreach (KeyValuePair<object, EventBean[]> entry in _repsPerGroup) {
	            if (_lastSeenKeys.Contains(entry.Key)) {
	                continue;
	            }
	            EventBean newEvent = _processor.GenerateOutputBatchedSingle(entry.Key, entry.Value, true, isSynthesize);
	            if (newEvent != null) {
	                _eventsNew.Add(newEvent);
	            }
	        }
	        _lastSeenKeys.Clear();

	        EventBean[] newEventsArr = _eventsNew.ToArrayOrNull();
	        EventBean[] oldEventsArr = null;
            if (_processor.Prototype.IsSelectRStream)
            {
                oldEventsArr = _eventsOld.ToArrayOrNull();
	        }
	        _eventsNew.Clear();
	        _eventsOld.Clear();
	        if ((newEventsArr == null) && (oldEventsArr == null)) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }
	}
} // end of namespace
