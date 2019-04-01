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
	public class ExprPreviousEvalStrategyPrev : ExprPreviousEvalStrategy {
	    private readonly int streamNumber;
	    private readonly ExprEvaluator indexNode;
	    private readonly ExprEvaluator evalNode;
	    private readonly RandomAccessByIndexGetter randomAccessGetter;
	    private readonly RelativeAccessByEventNIndexGetter relativeAccessGetter;
	    private readonly bool isConstantIndex;
	    private readonly int? constantIndexNumber;
	    private readonly bool isTail;

	    public ExprPreviousEvalStrategyPrev(int streamNumber, ExprEvaluator indexNode, ExprEvaluator evalNode, RandomAccessByIndexGetter randomAccessGetter, RelativeAccessByEventNIndexGetter relativeAccessGetter, bool constantIndex, int? constantIndexNumber, bool tail) {
	        this.streamNumber = streamNumber;
	        this.indexNode = indexNode;
	        this.evalNode = evalNode;
	        this.randomAccessGetter = randomAccessGetter;
	        this.relativeAccessGetter = relativeAccessGetter;
	        isConstantIndex = constantIndex;
	        this.constantIndexNumber = constantIndexNumber;
	        isTail = tail;
	    }

	    public object Evaluate(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
	        EventBean substituteEvent = GetSubstitute(eventsPerStream, exprEvaluatorContext);
	        if (substituteEvent == null) {
	            return null;
	        }

	        // Substitute original event with prior event, evaluate inner expression
	        EventBean originalEvent = eventsPerStream[streamNumber];
	        eventsPerStream[streamNumber] = substituteEvent;
	        object evalResult = evalNode.Evaluate(eventsPerStream, true, exprEvaluatorContext);
	        eventsPerStream[streamNumber] = originalEvent;

	        return evalResult;
	    }

	    public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return GetSubstitute(eventsPerStream, context);
	    }

	    public ICollection<EventBean> EvaluateGetCollEvents(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        return null;
	    }

	    public ICollection<object> EvaluateGetCollScalar(EventBean[] eventsPerStream, ExprEvaluatorContext context) {
	        object result = Evaluate(eventsPerStream, context);
	        if (result == null) {
	            return null;
	        }
	        return Collections.SingletonList(result);
	    }

	    private EventBean GetSubstitute(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {

	        // Use constant if supplied
	        int? index;
	        if (isConstantIndex) {
	            index = constantIndexNumber;
	        } else {
	            // evaluate first child, which returns the index
	            object indexResult = indexNode.Evaluate(eventsPerStream, true, exprEvaluatorContext);
	            if (indexResult == null) {
	                return null;
	            }
	            index = (indexResult).AsInt();
	        }

	        // access based on index returned
	        EventBean substituteEvent;
	        if (randomAccessGetter != null) {
	            RandomAccessByIndex randomAccess = randomAccessGetter.Accessor;
	            if (!isTail) {
	                substituteEvent = randomAccess.GetNewData(index.Value);
	            } else {
	                substituteEvent = randomAccess.GetNewDataTail(index.Value);
	            }
	        } else {
	            EventBean evalEvent = eventsPerStream[streamNumber];
	            RelativeAccessByEventNIndex relativeAccess = relativeAccessGetter.GetAccessor(evalEvent);
	            if (relativeAccess == null) {
	                return null;
	            }
	            if (!isTail) {
	                substituteEvent = relativeAccess.GetRelativeToEvent(evalEvent, index.Value);
	            } else {
	                substituteEvent = relativeAccess.GetRelativeToEnd(index.Value);
	            }
	        }
	        return substituteEvent;
	    }
	}
} // end of namespace