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
	public class ResultSetProcessorRowPerGroupOutputLastHelper
    {
        protected readonly ResultSetProcessorRowPerGroup processor;
	    private readonly IDictionary<object, EventBean[]> groupReps = new LinkedHashMap<object, EventBean[]>();
	    private readonly IDictionary<object, EventBean> groupRepsOutputLastUnordRStream = new LinkedHashMap<object, EventBean>();

	    public ResultSetProcessorRowPerGroupOutputLastHelper(ResultSetProcessorRowPerGroup processor) {
	        this.processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        if (newData != null) {
	            foreach (EventBean aNewData in newData) {
	                EventBean[] eventsPerStream = new EventBean[] {aNewData};
	                object mk = processor.GenerateGroupKey(eventsPerStream, true);

	                // if this is a newly encountered group, generate the remove stream event
                    if (groupReps.Push(mk, eventsPerStream) == null)
                    {
	                    if (processor.Prototype.IsSelectRStream) {
	                        processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, true, isGenerateSynthetic, groupRepsOutputLastUnordRStream);
	                    }
	                }
                    processor.AggregationService.ApplyEnter(eventsPerStream, mk, processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (EventBean anOldData in oldData) {
	                EventBean[] eventsPerStream = new EventBean[] {anOldData};
	                object mk = processor.GenerateGroupKey(eventsPerStream, true);

                    if (groupReps.Push(mk, eventsPerStream) == null)
                    {
                        if (processor.Prototype.IsSelectRStream)
                        {
	                        processor.GenerateOutputBatchedNoSortWMap(false, mk, eventsPerStream, false, isGenerateSynthetic, groupRepsOutputLastUnordRStream);
	                    }
	                }

                    processor.AggregationService.ApplyLeave(eventsPerStream, mk, processor.AgentInstanceContext);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic) {
	        if (newData != null) {
	            foreach (MultiKey<EventBean> aNewData in newData) {
	                object mk = processor.GenerateGroupKey(aNewData.Array, true);
                    if (groupReps.Push(mk, aNewData.Array) == null)
                    {
                        if (processor.Prototype.IsSelectRStream)
                        {
	                        processor.GenerateOutputBatchedNoSortWMap(true, mk, aNewData.Array, false, isGenerateSynthetic, groupRepsOutputLastUnordRStream);
	                    }
	                }
                    processor.AggregationService.ApplyEnter(aNewData.Array, mk, processor.AgentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (MultiKey<EventBean> anOldData in oldData) {
	                object mk = processor.GenerateGroupKey(anOldData.Array, false);
	                if (groupReps.Push(mk, anOldData.Array) == null) {
                        if (processor.Prototype.IsSelectRStream)
                        {
	                        processor.GenerateOutputBatchedNoSortWMap(true, mk, anOldData.Array, false, isGenerateSynthetic, groupRepsOutputLastUnordRStream);
	                    }
	                }
                    processor.AggregationService.ApplyLeave(anOldData.Array, mk, processor.AgentInstanceContext);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return Output(isSynthesize, false);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return Output(isSynthesize, true);
	    }

	    private UniformPair<EventBean[]> Output(bool isSynthesize, bool join) {
	        IList<EventBean> newEvents = new List<EventBean>(4);
	        processor.GenerateOutputBatchedArr(join, groupReps, true, isSynthesize, newEvents, null);
	        groupReps.Clear();
	        EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();

	        EventBean[] oldEventsArr = null;
	        if (groupRepsOutputLastUnordRStream != null && !groupRepsOutputLastUnordRStream.IsEmpty()) {
	            ICollection<EventBean> oldEvents = groupRepsOutputLastUnordRStream.Values;
	            oldEventsArr = oldEvents.ToArray();
	        }

	        if (newEventsArr == null && oldEventsArr == null) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }
	}
} // end of namespace
