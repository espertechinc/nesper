///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
	public class ExprPreviousEvalStrategyWindow : ExprPreviousEvalStrategy {
	    private readonly int streamNumber;
	    private readonly ExprEvaluator evalNode;
	    private readonly Type componentType;
	    private readonly RandomAccessByIndexGetter randomAccessGetter;
	    private readonly RelativeAccessByEventNIndexGetter relativeAccessGetter;

	    public ExprPreviousEvalStrategyWindow(int streamNumber, ExprEvaluator evalNode, Type componentType, RandomAccessByIndexGetter randomAccessGetter, RelativeAccessByEventNIndexGetter relativeAccessGetter) {
	        this.streamNumber = streamNumber;
	        this.evalNode = evalNode;
	        this.componentType = componentType;
	        this.randomAccessGetter = randomAccessGetter;
	        this.relativeAccessGetter = relativeAccessGetter;
	    }

	    public object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
	        IEnumerator<EventBean> events;
	        int size;
	        if (randomAccessGetter != null) {
	            RandomAccessByIndex randomAccess = randomAccessGetter.Accessor;
	            events = randomAccess.GetWindowEnumerator();
	            size = (int) randomAccess.WindowCount;
	        } else {
	            EventBean evalEvent = eventsPerStream[streamNumber];
	            RelativeAccessByEventNIndex relativeAccess = relativeAccessGetter.GetAccessor(evalEvent);
	            if (relativeAccess == null) {
	                return null;
	            }
	            size = relativeAccess.WindowToEventCount;
	            events = relativeAccess.WindowToEvent;
	        }

	        if (size <= 0) {
	            return null;
	        }

	        EventBean originalEvent = eventsPerStream[streamNumber];
	        Array result = Array.CreateInstance(componentType, size);

	        for (int i = 0; i < size; i++) {
	            events.MoveNext();
	            eventsPerStream[streamNumber] = events.Current;
	            result.SetValue(evalNode.Evaluate(eventsPerStream, true, exprEvaluatorContext), i);
	        }

	        eventsPerStream[streamNumber] = originalEvent;
	        return result;
	    }

	    public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        ICollection<EventBean> events;
	        if (randomAccessGetter != null) {
	            RandomAccessByIndex randomAccess = randomAccessGetter.Accessor;
	            events = randomAccess.WindowCollectionReadOnly;
	        } else {
	            EventBean evalEvent = eventsPerStream[streamNumber];
	            RelativeAccessByEventNIndex relativeAccess = relativeAccessGetter.GetAccessor(evalEvent);
	            if (relativeAccess == null) {
	                return null;
	            }
	            events = relativeAccess.WindowToEventCollReadOnly;
	        }
	        return events;
	    }

	    public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        IEnumerator<EventBean> events;
	        int size;
	        if (randomAccessGetter != null) {
	            RandomAccessByIndex randomAccess = randomAccessGetter.Accessor;
	            events = randomAccess.GetWindowEnumerator();
	            size = (int) randomAccess.WindowCount;
	        } else {
	            EventBean evalEvent = eventsPerStream[streamNumber];
	            RelativeAccessByEventNIndex relativeAccess = relativeAccessGetter.GetAccessor(evalEvent);
	            if (relativeAccess == null) {
	                return null;
	            }
	            size = relativeAccess.WindowToEventCount;
	            events = relativeAccess.WindowToEvent;
	        }

	        if (size <= 0) {
	            return Collections.GetEmptyList<object>();
	        }

	        EventBean originalEvent = eventsPerStream[streamNumber];
	        Deque<object> deque = new ArrayDeque<object>(size);
	        for (int i = 0; i < size; i++) {
	            events.MoveNext();
	            eventsPerStream[streamNumber] = events.Current;
	            object evalResult = evalNode.Evaluate(eventsPerStream, true, context);
	            deque.Add(evalResult);
	        }
	        eventsPerStream[streamNumber] = originalEvent;
	        return deque;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return null;
	    }
	}
} // end of namespace