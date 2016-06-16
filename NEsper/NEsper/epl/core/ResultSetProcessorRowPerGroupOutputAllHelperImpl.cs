///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowPerGroupOutputAllHelperImpl : ResultSetProcessorRowPerGroupOutputAllHelper
    {
	    private readonly ResultSetProcessorRowPerGroup _processor;

	    private readonly IDictionary<object, EventBean[]> _groupReps = new LinkedHashMap<object, EventBean[]>();
	    private readonly IDictionary<object, EventBean> _groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>();
	    private bool _first;

	    public ResultSetProcessorRowPerGroupOutputAllHelperImpl(ResultSetProcessorRowPerGroup processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, false);

	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                var eventsPerStream = new EventBean[] {aNewData};
	                var mk = _processor.GenerateGroupKey(eventsPerStream, true);
	                _groupReps.Put(mk, eventsPerStream);

	                if (_processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk)) {
	                    EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, true, isGenerateSynthetic);
	                    if (@event != null) {
	                        _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(eventsPerStream, mk, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                var eventsPerStream = new EventBean[] {anOldData};
	                var mk = _processor.GenerateGroupKey(eventsPerStream, true);

	                if (_processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk)) {
	                    EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, false, isGenerateSynthetic);
	                    if (@event != null) {
	                        _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                    }
	                }
	                _processor.AggregationService.ApplyLeave(eventsPerStream, mk, _processor.AgentInstanceContext);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, true);

	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                var mk = _processor.GenerateGroupKey(aNewData.Array, true);
	                _groupReps.Put(mk, aNewData.Array);

	                if (_processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk)) {
	                    EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(true, mk, aNewData.Array, true, isGenerateSynthetic);
	                    if (@event != null) {
	                        _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(aNewData.Array, mk, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                var mk = _processor.GenerateGroupKey(anOldData.Array, false);
	                if (_processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk)) {
	                    EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(true, mk, anOldData.Array, false, isGenerateSynthetic);
	                    if (@event != null) {
	                        _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                    }
	                }
	                _processor.AggregationService.ApplyLeave(anOldData.Array, mk, _processor.AgentInstanceContext);
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

	    public void Destroy() {
	        // no action required
	    }

	    private UniformPair<EventBean[]> Output(bool isSynthesize, bool join) {
	        // generate latest new-events from group representatives
	        IList<EventBean> newEvents = new List<EventBean>(4);
	        _processor.GenerateOutputBatchedArr(join, _groupReps.GetEnumerator(), true, isSynthesize, newEvents, null);
	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArrayOrNull();

	        // use old-events as retained, if any
	        EventBean[] oldEventsArr = null;
	        if (!_groupRepsOutputLastUnordRStream.IsEmpty()) {
	            var oldEvents = _groupRepsOutputLastUnordRStream.Values;
	            oldEventsArr = oldEvents.ToArrayOrNull();
	            _groupRepsOutputLastUnordRStream.Clear();
	        }
	        _first = true;

	        if (newEventsArr == null && oldEventsArr == null) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private void GenerateRemoveStreamJustOnce(bool isSynthesize, bool join) {
	        if (_first && _processor.Prototype.IsSelectRStream) {
	            foreach (var groupRep in _groupReps) {
	                var mk = _processor.GenerateGroupKey(groupRep.Value, false);
	                EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(join, mk, groupRep.Value, false, isSynthesize);
	                if (@event != null) {
	                    _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                }
	            }
	        }
	        _first = false;
	    }
	}
} // end of namespace
