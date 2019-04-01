///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.expression.prev
{
	public class ExprPreviousEvalStrategyCount : ExprPreviousEvalStrategy {
	    private readonly int streamNumber;
	    private readonly RandomAccessByIndexGetter randomAccessGetter;
	    private readonly RelativeAccessByEventNIndexGetter relativeAccessGetter;

	    public ExprPreviousEvalStrategyCount(int streamNumber, RandomAccessByIndexGetter randomAccessGetter, RelativeAccessByEventNIndexGetter relativeAccessGetter) {
	        this.streamNumber = streamNumber;
	        this.randomAccessGetter = randomAccessGetter;
	        this.relativeAccessGetter = relativeAccessGetter;
	    }

	    public object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
	        long size;
	        if (randomAccessGetter != null) {
	            RandomAccessByIndex randomAccess = randomAccessGetter.Accessor;
	            size = randomAccess.WindowCount;
	        } else {
	            EventBean evalEvent = eventsPerStream[streamNumber];
	            RelativeAccessByEventNIndex relativeAccess = relativeAccessGetter.GetAccessor(evalEvent);
	            if (relativeAccess == null) {
	                return null;
	            }
	            size = relativeAccess.WindowToEventCount;
	        }

	        return size;
	    }

	    public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return null;
	    }

	    public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return null;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return null;
	    }
	}
} // end of namespace