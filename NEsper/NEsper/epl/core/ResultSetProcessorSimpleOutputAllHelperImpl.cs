///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
	public class ResultSetProcessorSimpleOutputAllHelperImpl : ResultSetProcessorSimpleOutputAllHelper
	{
	    private readonly ResultSetProcessorSimple processor;

	    private readonly Deque<EventBean> eventsNewView = new ArrayDeque<EventBean>(2);
	    private readonly Deque<EventBean> eventsOldView = new ArrayDeque<EventBean>(2);
	    private readonly Deque<MultiKey<EventBean>> eventsNewJoin = new ArrayDeque<MultiKey<EventBean>>(2);
	    private readonly Deque<MultiKey<EventBean>> eventsOldJoin = new ArrayDeque<MultiKey<EventBean>>(2);

	    public ResultSetProcessorSimpleOutputAllHelperImpl(ResultSetProcessorSimple processor) {
	        this.processor = processor;
	    }

	    public void ProcessView(EventBean[] newData, EventBean[] oldData) {
	        if (processor.Prototype.OptionalHavingExpr == null) {
	            AddToView(newData, oldData);
	            return;
	        }

	        var eventsPerStream = new EventBean[1];
	        if (newData != null && newData.Length > 0) {
                var evaluateParams = new EvaluateParams(eventsPerStream, true, processor.ExprEvaluatorContext);
                foreach (var theEvent in newData) {
	                eventsPerStream[0] = theEvent;

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
	                var passesHaving = processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean());}
                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                eventsNewView.Add(theEvent);
	            }
	        }
	        if (oldData != null && oldData.Length > 0) {
                var evaluateParams = new EvaluateParams(eventsPerStream, false, processor.ExprEvaluatorContext);
                foreach (var theEvent in oldData) {
	                eventsPerStream[0] = theEvent;

	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
                    var passesHaving = processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
                        continue;
	                }
	                eventsOldView.Add(theEvent);
	            }
	        }
	    }

	    public void ProcessJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        if (processor.Prototype.OptionalHavingExpr == null) {
	            AddToJoin(newEvents, oldEvents);
	            return;
	        }

	        if (newEvents != null && newEvents.Count > 0) {
	            foreach (var theEvent in newEvents) {
                    var evaluateParams = new EvaluateParams(theEvent.Array, true, processor.ExprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array); }
                    var passesHaving = processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
                        continue;
	                }
	                eventsNewJoin.Add(theEvent);
	            }
	        }
	        if (oldEvents != null && oldEvents.Count > 0) {
	            foreach (var theEvent in oldEvents) {
                    var evaluateParams = new EvaluateParams(theEvent.Array, false, processor.ExprEvaluatorContext);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(theEvent.Array);}
                    var passesHaving = processor.Prototype.OptionalHavingExpr.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                    if ((passesHaving == null) || (false.Equals(passesHaving))) {
	                    continue;
	                }
	                eventsOldJoin.Add(theEvent);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> OutputView(bool isSynthesize) {
	        var pair = processor.ProcessViewResult(EventBeanUtility.ToArrayNullIfEmpty(eventsNewView), EventBeanUtility.ToArrayNullIfEmpty(eventsOldView), isSynthesize);
	        eventsNewView.Clear();
	        eventsOldView.Clear();
	        return pair;
	    }

	    public UniformPair<EventBean[]> OutputJoin(bool isSynthesize) {
	        var pair = processor.ProcessJoinResult(EventBeanUtility.ToLinkedHashSetNullIfEmpty(eventsNewJoin), EventBeanUtility.ToLinkedHashSetNullIfEmpty(eventsOldJoin), isSynthesize);
	        eventsNewJoin.Clear();
	        eventsOldJoin.Clear();
	        return pair;
	    }

	    public void Destroy() {
	        // no action required
	    }

	    private void AddToView(EventBean[] newData, EventBean[] oldData) {
	        EventBeanUtility.AddToCollection(newData, eventsNewView);
	        EventBeanUtility.AddToCollection(oldData, eventsOldView);
	    }

	    private void AddToJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        EventBeanUtility.AddToCollection(newEvents, eventsNewJoin);
	        EventBeanUtility.AddToCollection(oldEvents, eventsOldJoin);
	    }
	}
} // end of namespace
