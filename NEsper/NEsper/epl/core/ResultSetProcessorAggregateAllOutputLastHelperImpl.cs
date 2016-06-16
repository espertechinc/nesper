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

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorAggregateAllOutputLastHelperImpl : ResultSetProcessorAggregateAllOutputLastHelper
	{
	    private readonly ResultSetProcessorAggregateAll _processor;

	    private EventBean _lastEventIStreamForOutputLast;
	    private EventBean _lastEventRStreamForOutputLast;

	    public ResultSetProcessorAggregateAllOutputLastHelperImpl(ResultSetProcessorAggregateAll processor) {
	        this._processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        var pair = _processor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
	        Apply(pair);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        var pair = _processor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
	        Apply(pair);
	    }

	    public UniformPair<EventBean[]> Output() {
	        UniformPair<EventBean[]> newOldEvents = null;
	        if (_lastEventIStreamForOutputLast != null) {
	            var istream = new EventBean[] {_lastEventIStreamForOutputLast};
	            newOldEvents = new UniformPair<EventBean[]>(istream, null);
	        }
	        if (_lastEventRStreamForOutputLast != null) {
	            var rstream = new EventBean[] {_lastEventRStreamForOutputLast};
	            if (newOldEvents == null) {
	                newOldEvents = new UniformPair<EventBean[]>(null, rstream);
	            }
	            else {
	                newOldEvents.Second = rstream;
	            }
	        }

	        _lastEventIStreamForOutputLast = null;
	        _lastEventRStreamForOutputLast = null;
	        return newOldEvents;
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private void Apply(UniformPair<EventBean[]> pair) {
	        if (pair == null) {
	            return;
	        }
	        if (pair.First != null && pair.First.Length > 0) {
	            _lastEventIStreamForOutputLast = pair.First[pair.First.Length - 1];
	        }
	        if (pair.Second != null && pair.Second.Length > 0) {
	            _lastEventRStreamForOutputLast = pair.Second[pair.Second.Length - 1];
	        }
	    }
	}
} // end of namespace
