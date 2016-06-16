///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
	public class ResultSetProcessorRowForAllOutputAllHelperImpl : ResultSetProcessorRowForAllOutputAllHelper
	{
	    private readonly ResultSetProcessorRowForAll processor;
	    private readonly Deque<EventBean> eventsOld = new ArrayDeque<EventBean>(2);
	    private readonly Deque<EventBean> eventsNew = new ArrayDeque<EventBean>(2);

	    public ResultSetProcessorRowForAllOutputAllHelperImpl(ResultSetProcessorRowForAll processor) {
	        this.processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        if (processor.Prototype.IsSelectRStream) {
	            EventBean[] eventsX = processor.GetSelectListEvents(false, isGenerateSynthetic, false);
	            EventBeanUtility.AddToCollection(eventsX, eventsOld);
	        }

	        EventBean[] eventsPerStream = new EventBean[1];
	        ResultSetProcessorUtil.ApplyAggViewResult(processor.AggregationService, processor._exprEvaluatorContext, newData, oldData, eventsPerStream);

	        EventBean[] events = processor.GetSelectListEvents(true, isGenerateSynthetic, false);
	        EventBeanUtility.AddToCollection(events, eventsNew);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        if (processor.Prototype.IsSelectRStream) {
	            EventBean[] eventsX = processor.GetSelectListEvents(false, isGenerateSynthetic, true);
	            EventBeanUtility.AddToCollection(eventsX, eventsOld);
	        }

	        ResultSetProcessorUtil.ApplyAggJoinResult(processor.AggregationService, processor._exprEvaluatorContext, newEvents, oldEvents);

	        EventBean[] events = processor.GetSelectListEvents(true, isGenerateSynthetic, true);
	        EventBeanUtility.AddToCollection(events, eventsNew);
	    }

	    public UniformPair<EventBean[]> OutputView(bool isGenerateSynthetic) {
	        return Output(isGenerateSynthetic, false);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isGenerateSynthetic) {
	        return Output(isGenerateSynthetic, true);
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private UniformPair<EventBean[]> Output(bool isGenerateSynthetic, bool isJoin) {
	        EventBean[] oldEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsOld);
	        EventBean[] newEvents = EventBeanUtility.ToArrayNullIfEmpty(eventsNew);

	        if (newEvents == null) {
	            newEvents = processor.GetSelectListEvents(true, isGenerateSynthetic, isJoin);
	        }
	        if (oldEvents == null && processor.Prototype.IsSelectRStream) {
	            oldEvents = processor.GetSelectListEvents(false, isGenerateSynthetic, isJoin);
	        }

	        UniformPair<EventBean[]> result = null;
	        if (oldEvents != null || newEvents != null) {
	            result = new UniformPair<EventBean[]>(newEvents, oldEvents);
	        }

	        eventsOld.Clear();
	        eventsNew.Clear();
	        return result;
	    }
	}
} // end of namespace
