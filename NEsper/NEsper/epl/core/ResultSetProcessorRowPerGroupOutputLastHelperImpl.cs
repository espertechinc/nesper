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
	public class ResultSetProcessorRowPerGroupOutputLastHelperImpl : ResultSetProcessorRowPerGroupOutputLastHelper
    {
	    private readonly ResultSetProcessorRowPerGroup _processor;
	    private readonly IDictionary<object, EventBean[]> _groupReps = new LinkedHashMap<object, EventBean[]>();
	    private readonly IDictionary<object, EventBean> _groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>();

	    public ResultSetProcessorRowPerGroupOutputLastHelperImpl(ResultSetProcessorRowPerGroup processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        if (newData != null) {
	            foreach (EventBean aNewData in newData) {
	                EventBean[] eventsPerStream = new EventBean[] {aNewData};
	                object mk = _processor.GenerateGroupKey(eventsPerStream, true);

	                // if this is a newly encountered group, generate the remove stream event
	                if (_groupReps.Push(mk, eventsPerStream) == null) {
	                    if (_processor.Prototype.IsSelectRStream) {
	                        EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, true, isGenerateSynthetic);
	                        if (@event != null) {
	                            _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(eventsPerStream, mk, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (EventBean anOldData in oldData) {
	                EventBean[] eventsPerStream = new EventBean[] {anOldData};
	                object mk = _processor.GenerateGroupKey(eventsPerStream, true);

	                if (_groupReps.Push(mk, eventsPerStream) == null) {
	                    if (_processor.Prototype.IsSelectRStream) {
	                        EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, false, isGenerateSynthetic);
	                        if (@event != null) {
	                            _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                        }
	                    }
	                }

                    _processor.AggregationService.ApplyLeave(eventsPerStream, mk, _processor.AgentInstanceContext);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        if (newData != null) {
	            foreach (MultiKey<EventBean> aNewData in newData) {
	                object mk = _processor.GenerateGroupKey(aNewData.Array, true);
	                if (_groupReps.Push(mk, aNewData.Array) == null) {
	                    if (_processor.Prototype.IsSelectRStream) {
	                        EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(true, mk, aNewData.Array, false, isGenerateSynthetic);
	                        if (@event != null) {
	                            _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyEnter(aNewData.Array, mk, _processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (MultiKey<EventBean> anOldData in oldData) {
	                object mk = _processor.GenerateGroupKey(anOldData.Array, false);
	                if (_groupReps.Push(mk, anOldData.Array) == null) {
	                    if (_processor.Prototype.IsSelectRStream) {
	                        EventBean @event = _processor.GenerateOutputBatchedNoSortWMap(true, mk, anOldData.Array, false, isGenerateSynthetic);
	                        if (@event != null) {
	                            _groupRepsOutputLastUnordRStream.Put(mk, @event);
	                        }
	                    }
	                }
	                _processor.AggregationService.ApplyLeave(anOldData.Array, mk, _processor.AgentInstanceContext);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return Output(isSynthesize, false);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return Output(isSynthesize, true);
	    }

	    public void Destroy() {
	        // no action required
	    }

	    public void Remove(object key) {
	        _groupReps.Remove(key);
	    }

	    private UniformPair<EventBean[]> Output(bool isSynthesize, bool join) {
	        IList<EventBean> newEvents = new List<EventBean>(4);
	        _processor.GenerateOutputBatchedArr(join, _groupReps.GetEnumerator(), true, isSynthesize, newEvents, null);
	        _groupReps.Clear();
	        EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArrayOrNull();

	        EventBean[] oldEventsArr = null;
	        if (_groupRepsOutputLastUnordRStream != null && !_groupRepsOutputLastUnordRStream.IsEmpty()) {
	            ICollection<EventBean> oldEvents = _groupRepsOutputLastUnordRStream.Values;
                oldEventsArr = oldEvents.ToArrayOrNull();
	        }

	        if (newEventsArr == null && oldEventsArr == null) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }
	}
} // end of namespace
