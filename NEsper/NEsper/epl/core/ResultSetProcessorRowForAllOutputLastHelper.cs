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

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowForAllOutputLastHelper
	{
	    private readonly ResultSetProcessorRowForAll _processor;
	    private EventBean[] _lastEventRStreamForOutputLast;

	    public ResultSetProcessorRowForAllOutputLastHelper(ResultSetProcessorRowForAll processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic) {
	        if (_processor.Prototype.IsSelectRStream && _lastEventRStreamForOutputLast == null) {
	            _lastEventRStreamForOutputLast = _processor.GetSelectListEvents(false, isGenerateSynthetic, false);
	        }

	        EventBean[] eventsPerStream = new EventBean[1];
	        ResultSetProcessorUtil.ApplyAggViewResult(_processor.AggregationService, _processor.ExprEvaluatorContext, newData, oldData, eventsPerStream);
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic) {
	        if (_processor.Prototype.IsSelectRStream && _lastEventRStreamForOutputLast == null) {
	            _lastEventRStreamForOutputLast = _processor.GetSelectListEvents(false, isGenerateSynthetic, true);
	        }

	        ResultSetProcessorUtil.ApplyAggJoinResult(_processor.AggregationService, _processor.ExprEvaluatorContext, newEvents, oldEvents);
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered(isSynthesize);
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        return ContinueOutputLimitedLastNonBuffered(isSynthesize);
	    }

	    private UniformPair<EventBean[]> ContinueOutputLimitedLastNonBuffered(bool isSynthesize) {
	        EventBean[] events = _processor.GetSelectListEvents(true, isSynthesize, false);
	        UniformPair<EventBean[]> result = new UniformPair<EventBean[]>(events, null);

	        if (_processor.Prototype.IsSelectRStream && _lastEventRStreamForOutputLast == null) {
	            _lastEventRStreamForOutputLast = _processor.GetSelectListEvents(false, isSynthesize, false);
	        }
	        if (_lastEventRStreamForOutputLast != null) {
	            result.Second = _lastEventRStreamForOutputLast;
	            _lastEventRStreamForOutputLast = null;
	        }

	        return result;
	    }

	}
} // end of namespace
