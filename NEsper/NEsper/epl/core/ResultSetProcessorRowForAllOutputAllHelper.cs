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
using com.espertech.esper.events;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowForAllOutputAllHelper
	{
	    private readonly ResultSetProcessorRowForAll _processor;
	    private readonly Deque<EventBean> _eventsOld = new ArrayDeque<EventBean>(2);
	    private readonly Deque<EventBean> _eventsNew = new ArrayDeque<EventBean>(2);

	    public ResultSetProcessorRowForAllOutputAllHelper(ResultSetProcessorRowForAll processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic)
	    {
	        EventBean[] events;

	        if (_processor.Prototype.IsSelectRStream) {
	            events = _processor.GetSelectListEvents(false, isGenerateSynthetic, false);
	            EventBeanUtility.AddToCollection(events, _eventsOld);
	        }

	        EventBean[] eventsPerStream = new EventBean[1];
	        ResultSetProcessorUtil.ApplyAggViewResult(_processor.AggregationService, _processor.ExprEvaluatorContext, newData, oldData, eventsPerStream);

	        events = _processor.GetSelectListEvents(true, isGenerateSynthetic, false);
	        EventBeanUtility.AddToCollection(events, _eventsNew);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic)
	    {
	        EventBean[] events;

	        if (_processor.Prototype.IsSelectRStream) {
	            events = _processor.GetSelectListEvents(false, isGenerateSynthetic, true);
	            EventBeanUtility.AddToCollection(events, _eventsOld);
	        }

	        ResultSetProcessorUtil.ApplyAggJoinResult(_processor.AggregationService, _processor.ExprEvaluatorContext, newEvents, oldEvents);

	        events = _processor.GetSelectListEvents(true, isGenerateSynthetic, true);
	        EventBeanUtility.AddToCollection(events, _eventsNew);
	    }

	    public UniformPair<EventBean[]> OutputView(bool isGenerateSynthetic) {
	        return Output(isGenerateSynthetic, false);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isGenerateSynthetic) {
	        return Output(isGenerateSynthetic, true);
	    }

	    private UniformPair<EventBean[]> Output(bool isGenerateSynthetic, bool isJoin) {
	        EventBean[] oldEvents = EventBeanUtility.ToArrayNullIfEmpty(_eventsOld);
	        EventBean[] newEvents = EventBeanUtility.ToArrayNullIfEmpty(_eventsNew);

	        if (newEvents == null) {
	            newEvents = _processor.GetSelectListEvents(true, isGenerateSynthetic, isJoin);
	        }
	        if (oldEvents == null && _processor.Prototype.IsSelectRStream) {
	            oldEvents = _processor.GetSelectListEvents(false, isGenerateSynthetic, isJoin);
	        }

	        UniformPair<EventBean[]> result = null;
	        if (oldEvents != null || newEvents != null) {
	            result = new UniformPair<EventBean[]>(newEvents, oldEvents);
	        }

	        _eventsOld.Clear();
	        _eventsNew.Clear();
	        return result;
	    }
	}
} // end of namespace
