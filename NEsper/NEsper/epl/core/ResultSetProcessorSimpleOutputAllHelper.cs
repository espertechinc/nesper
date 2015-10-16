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
	public class ResultSetProcessorSimpleOutputAllHelper
	{
	    private readonly ResultSetProcessorSimple _processor;

	    private readonly Deque<EventBean> _eventsNewView = new ArrayDeque<EventBean>(2);
	    private readonly Deque<EventBean> _eventsOldView = new ArrayDeque<EventBean>(2);
	    private readonly Deque<MultiKey<EventBean>> _eventsNewJoin = new ArrayDeque<MultiKey<EventBean>>(2);
	    private readonly Deque<MultiKey<EventBean>> _eventsOldJoin = new ArrayDeque<MultiKey<EventBean>>(2);

	    public ResultSetProcessorSimpleOutputAllHelper(ResultSetProcessorSimple processor) {
	        _processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData) {
	        if (_processor.Prototype.OptionalHavingExpr == null) {
	            AddToView(newData, oldData);
	            return;
	        }

	        var eventsPerStream = new EventBean[1];
	        var eventParams = new EvaluateParams(eventsPerStream, true, _processor.ExprEvaluatorContext);
	        if (newData != null && newData.Length > 0) {
	            foreach (var theEvent in newData) {
	                eventsPerStream[0] = theEvent;

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
	                var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(eventParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean());}
	                if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                _eventsNewView.Add(theEvent);
	            }
	        }

            eventParams = new EvaluateParams(eventsPerStream, false, _processor.ExprEvaluatorContext);
	        if (oldData != null && oldData.Length > 0) {
	            foreach (var theEvent in oldData) {
	                eventsPerStream[0] = theEvent;

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
	                var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(eventParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean());}
                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                _eventsOldView.Add(theEvent);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        if (_processor.Prototype.OptionalHavingExpr == null) {
	            AddToJoin(newEvents, oldEvents);
	            return;
	        }

            if (newEvents != null && newEvents.Count > 0) {
	            foreach (var theEvent in newEvents) {
                    var eventParams = new EvaluateParams(theEvent.Array, true, _processor.ExprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array); }
                    var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(eventParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean());}
	                if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                _eventsNewJoin.Add(theEvent);
	            }
	        }
	        if (oldEvents != null && oldEvents.Count > 0) {
	            foreach (var theEvent in oldEvents) {
                    var eventParams = new EvaluateParams(theEvent.Array, false, _processor.ExprEvaluatorContext);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array);}
	                var passesHaving = _processor.Prototype.OptionalHavingExpr.Evaluate(eventParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean());}
	                if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                _eventsOldJoin.Add(theEvent);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        var pair = _processor.ProcessViewResult(EventBeanUtility.ToArrayNullIfEmpty(_eventsNewView), EventBeanUtility.ToArrayNullIfEmpty(_eventsOldView), isSynthesize);
	        _eventsNewView.Clear();
	        _eventsOldView.Clear();
	        return pair;
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        var pair = _processor.ProcessJoinResult(EventBeanUtility.ToLinkedHashSetNullIfEmpty(_eventsNewJoin), EventBeanUtility.ToLinkedHashSetNullIfEmpty(_eventsOldJoin), isSynthesize);
	        _eventsNewJoin.Clear();
	        _eventsOldJoin.Clear();
	        return pair;
	    }

	    private void AddToView(EventBean[] newData, EventBean[] oldData) {
	        EventBeanUtility.AddToCollection(newData, _eventsNewView);
	        EventBeanUtility.AddToCollection(oldData, _eventsOldView);
	    }

	    private void AddToJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        EventBeanUtility.AddToCollection(newEvents, _eventsNewJoin);
	        EventBeanUtility.AddToCollection(oldEvents, _eventsOldJoin);
	    }
	}
} // end of namespace
