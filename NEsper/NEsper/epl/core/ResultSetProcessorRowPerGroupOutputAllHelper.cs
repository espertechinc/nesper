///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowPerGroupOutputAllHelper
    {
	    internal readonly ResultSetProcessorRowPerGroup Processor;

	    private readonly IDictionary<object, EventBean[]> _groupReps = new LinkedHashMap<object, EventBean[]>();
	    private readonly IDictionary<object, EventBean> _groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>();
	    private bool _first;

	    public ResultSetProcessorRowPerGroupOutputAllHelper(ResultSetProcessorRowPerGroup processor) {
	        Processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, false);

	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                var eventsPerStream = new EventBean[] {aNewData};
	                var mk = Processor.GenerateGroupKey(eventsPerStream, true);
	                _groupReps.Put(mk, eventsPerStream);

                    if (Processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk))
                    {
	                    Processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, true, isGenerateSynthetic, _groupRepsOutputLastUnordRStream);
	                }
	                Processor.AggregationService.ApplyEnter(eventsPerStream, mk, Processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                var eventsPerStream = new EventBean[] {anOldData};
	                var mk = Processor.GenerateGroupKey(eventsPerStream, true);

	                if (Processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk)) {
	                    Processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, false, isGenerateSynthetic, _groupRepsOutputLastUnordRStream);
	                }
	                Processor.AggregationService.ApplyLeave(eventsPerStream, mk, Processor.AgentInstanceContext);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        GenerateRemoveStreamJustOnce(isGenerateSynthetic, true);

	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                var mk = Processor.GenerateGroupKey(aNewData.Array, true);
	                _groupReps.Put(mk, aNewData.Array);

                    if (Processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk))
                    {
	                    Processor.GenerateOutputBatchedNoSortWMap(true, mk, aNewData.Array, true, isGenerateSynthetic, _groupRepsOutputLastUnordRStream);
	                }
	                Processor.AggregationService.ApplyEnter(aNewData.Array, mk, Processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                var mk = Processor.GenerateGroupKey(anOldData.Array, false);
                    if (Processor.Prototype.IsSelectRStream && !_groupRepsOutputLastUnordRStream.ContainsKey(mk))
                    {
	                    Processor.GenerateOutputBatchedNoSortWMap(true, mk, anOldData.Array, false, isGenerateSynthetic, _groupRepsOutputLastUnordRStream);
	                }
	                Processor.AggregationService.ApplyLeave(anOldData.Array, mk, Processor.AgentInstanceContext);
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

	    private UniformPair<EventBean[]> Output(bool isSynthesize, bool join) {
	        // generate latest new-events from group representatives
	        IList<EventBean> newEvents = new List<EventBean>(4);
	        Processor.GenerateOutputBatchedArr(join, _groupReps, true, isSynthesize, newEvents, null);
	        EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();

	        // use old-events as retained, if any
	        EventBean[] oldEventsArr = null;
	        if (!_groupRepsOutputLastUnordRStream.IsEmpty()) {
	            ICollection<EventBean> oldEvents = _groupRepsOutputLastUnordRStream.Values;
	            oldEventsArr = oldEvents.ToArray();
	            _groupRepsOutputLastUnordRStream.Clear();
	        }
	        _first = true;

	        if (newEventsArr == null && oldEventsArr == null) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private void GenerateRemoveStreamJustOnce(bool isSynthesize, bool join) {
            if (_first && Processor.Prototype.IsSelectRStream)
            {
	            foreach (var groupRep in _groupReps)
                {
	                var mk = Processor.GenerateGroupKey(groupRep.Value, false);
	                Processor.GenerateOutputBatchedNoSortWMap(join, mk, groupRep.Value, false, isSynthesize, _groupRepsOutputLastUnordRStream);
	            }
	        }
	        _first = false;
	    }
	}
} // end of namespace
