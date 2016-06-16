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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorSimpleOutputLastHelperImpl : ResultSetProcessorSimpleOutputLastHelper
	{
	    private readonly ResultSetProcessorSimple _processor;

	    private EventBean _outputLastIStreamBufView;
	    private EventBean _outputLastRStreamBufView;
	    private MultiKey<EventBean> _outputLastIStreamBufJoin;
	    private MultiKey<EventBean> _outputLastRStreamBufJoin;

	    public ResultSetProcessorSimpleOutputLastHelperImpl(ResultSetProcessorSimple processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData) {
	        if (_processor.Prototype.OptionalHavingExpr == null) {
	            if (newData != null && newData.Length > 0) {
	                _outputLastIStreamBufView = newData[newData.Length - 1];
	            }
	            if (oldData != null && oldData.Length > 0) {
	                _outputLastRStreamBufView = oldData[oldData.Length - 1];
	            }
	        }
	        else {
	            EventBean[] eventsPerStream = new EventBean[1];
	            if (newData != null && newData.Length > 0) {
                    var evaluateParams = new EvaluateParams(eventsPerStream, true, _processor.ExprEvaluatorContext);
                    foreach (EventBean theEvent in newData)
                    {
	                    eventsPerStream[0] = theEvent;

	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
	                    var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
	                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                        continue;
	                    }
	                    _outputLastIStreamBufView = theEvent;
	                }
	            }
	            if (oldData != null && oldData.Length > 0) {
                    var evaluateParams = new EvaluateParams(eventsPerStream, false, _processor.ExprEvaluatorContext);
                    foreach (EventBean theEvent in oldData)
                    {
	                    eventsPerStream[0] = theEvent;

	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
	                    var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
	                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                        continue;
	                    }
	                    _outputLastRStreamBufView = theEvent;
	                }
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        if (_processor.Prototype.OptionalHavingExpr == null) {
	            if (newEvents != null && !newEvents.IsEmpty()) {
	                _outputLastIStreamBufJoin = EventBeanUtility.GetLastInSet(newEvents);
	            }
	            if (oldEvents != null && !oldEvents.IsEmpty()) {
	                _outputLastRStreamBufJoin = EventBeanUtility.GetLastInSet(oldEvents);
	            }
	        }
	        else {
	            if (newEvents != null && newEvents.Count > 0) {
	                foreach (MultiKey<EventBean> theEvent in newEvents) {
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array);}
	                    var evaluateParams = new EvaluateParams(theEvent.Array, true, _processor.ExprEvaluatorContext);
	                    var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
	                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                        continue;
	                    }
	                    _outputLastIStreamBufJoin = theEvent;
	                }
	            }
	            if (oldEvents != null && oldEvents.Count > 0) {
	                foreach (MultiKey<EventBean> theEvent in oldEvents) {
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array);}
                        var evaluateParams = new EvaluateParams(theEvent.Array, false, _processor.ExprEvaluatorContext);
                        var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
	                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                        continue;
	                    }
	                    _outputLastRStreamBufJoin = theEvent;
	                }
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        if (_outputLastIStreamBufView == null && _outputLastRStreamBufView == null) {
	            return null;
	        }
	        UniformPair<EventBean[]> pair = _processor.ProcessViewResult(EventBeanUtility.ToArrayIfNotNull(_outputLastIStreamBufView), EventBeanUtility.ToArrayIfNotNull(_outputLastRStreamBufView), isSynthesize);
	        _outputLastIStreamBufView = null;
	        _outputLastRStreamBufView = null;
	        return pair;
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        if (_outputLastIStreamBufJoin == null && _outputLastRStreamBufJoin == null) {
	            return null;
	        }
	        UniformPair<EventBean[]> pair = _processor.ProcessJoinResult(EventBeanUtility.ToSingletonSetIfNotNull(_outputLastIStreamBufJoin), EventBeanUtility.ToSingletonSetIfNotNull(_outputLastRStreamBufJoin), isSynthesize);
	        _outputLastIStreamBufJoin = null;
	        _outputLastRStreamBufJoin = null;
	        return pair;
	    }

	    public void Destroy() {
	        // no action required
	    }
	}
} // end of namespace
