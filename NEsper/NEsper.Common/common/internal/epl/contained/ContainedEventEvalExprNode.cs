///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.contained
{
	public class ContainedEventEvalExprNode : ContainedEventEval {

	    private readonly ExprEvaluator evaluator;
	    private readonly EventBeanFactory eventBeanFactory;

	    public ContainedEventEvalExprNode(
	        ExprEvaluator evaluator, 
	        EventType eventType, 
	        EPStatementInitServices initServices) {
	        this.evaluator = evaluator;
	        this.eventBeanFactory = EventTypeUtility.GetFactoryForType(eventType, initServices.EventBeanTypedEventFactory, initServices.EventTypeAvroHandler);
	    }

	    public object GetFragment(
	        EventBean eventBean, 
	        EventBean[] eventsPerStream, 
	        ExprEvaluatorContext exprEvaluatorContext) {
	        object result = evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);

	        if (result == null) {
	            return null;
	        }

            if (result is Array asArray) {
	            EventBean[] events = new EventBean[asArray.Length];
	            for (int i = 0; i < events.Length; i++) {
	                var arrayItem = asArray.GetValue(i);
	                if (arrayItem != null) {
	                    events[i] = eventBeanFactory.Wrap(arrayItem);
	                }
	            }
	            return events;
	        }

            if (result.GetType().IsGenericCollection()) {
                return result
                    .Unwrap<object>()
                    .Select(item => item != null ? eventBeanFactory.Wrap(item) : null)
                    .ToArray();
	        }

	        if (result.GetType().IsGenericEnumerable()) {
	            return result
	                .UnwrapEnumerable<object>()
	                .Where(item => item != null)
	                .ToArray();
	        }

            return null;
	    }
	}
} // end of namespace