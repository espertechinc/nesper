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
	public class ResultSetProcessorRowForAllOutputLastHelperImpl : ResultSetProcessorRowForAllOutputLastHelper
	{
	    private readonly ResultSetProcessorRowForAll processor;
	    private EventBean[] lastEventRStreamForOutputLast;

	    public ResultSetProcessorRowForAllOutputLastHelperImpl(ResultSetProcessorRowForAll processor) {
	        this.processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        if (processor.Prototype.IsSelectRStream && lastEventRStreamForOutputLast == null) {
	            lastEventRStreamForOutputLast = processor.GetSelectListEvents(false, isGenerateSynthetic, false);
	        }

	        EventBean[] eventsPerStream = new EventBean[1];
	        ResultSetProcessorUtil.ApplyAggViewResult(processor.AggregationService, processor._exprEvaluatorContext, newData, oldData, eventsPerStream);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        if (processor.Prototype.IsSelectRStream && lastEventRStreamForOutputLast == null) {
	            lastEventRStreamForOutputLast = processor.GetSelectListEvents(false, isGenerateSynthetic, true);
	        }

	        ResultSetProcessorUtil.ApplyAggJoinResult(processor.AggregationService, processor._exprEvaluatorContext, newEvents, oldEvents);
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered(isSynthesize);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered(isSynthesize);
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private UniformPair<EventBean[]> ContinueOutputLimitedLastNonBuffered(bool isSynthesize) {
	        EventBean[] events = processor.GetSelectListEvents(true, isSynthesize, false);
	        UniformPair<EventBean[]> result = new UniformPair<EventBean[]>(events, null);

	        if (processor.Prototype.IsSelectRStream && lastEventRStreamForOutputLast == null) {
	            lastEventRStreamForOutputLast = processor.GetSelectListEvents(false, isSynthesize, false);
	        }
	        if (lastEventRStreamForOutputLast != null) {
	            result.Second = lastEventRStreamForOutputLast;
	            lastEventRStreamForOutputLast = null;
	        }

	        return result;
	    }
	}
} // end of namespace
