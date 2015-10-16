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
	public class ResultSetProcessorAggregateAllOutputAllHelper
	{
	    private readonly ResultSetProcessorAggregateAll _processor;
	    private readonly Deque<EventBean> _eventsOld = new ArrayDeque<EventBean>(2);
	    private readonly Deque<EventBean> _eventsNew = new ArrayDeque<EventBean>(2);

	    public ResultSetProcessorAggregateAllOutputAllHelper(ResultSetProcessorAggregateAll processor) {
	        this._processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        UniformPair<EventBean[]> pair = _processor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
	        Apply(pair);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        UniformPair<EventBean[]> pair = _processor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
	        Apply(pair);
	    }

	    public UniformPair<EventBean[]> Output() {
	        EventBean[] oldEvents = EventBeanUtility.ToArrayNullIfEmpty(_eventsOld);
	        EventBean[] newEvents = EventBeanUtility.ToArrayNullIfEmpty(_eventsNew);

	        UniformPair<EventBean[]> result = null;
	        if (oldEvents != null || newEvents != null) {
	            result = new UniformPair<EventBean[]>(newEvents, oldEvents);
	        }

	        _eventsOld.Clear();
	        _eventsNew.Clear();
	        return result;
	    }

	    private void Apply(UniformPair<EventBean[]> pair) {
	        if (pair == null) {
	            return;
	        }
	        EventBeanUtility.AddToCollection(pair.First, _eventsNew);
	        EventBeanUtility.AddToCollection(pair.Second, _eventsOld);
	    }
	}
} // end of namespace
