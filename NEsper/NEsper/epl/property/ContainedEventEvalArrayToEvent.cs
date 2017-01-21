///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.property
{
	public class ContainedEventEvalArrayToEvent : ContainedEventEval
    {
	    private readonly ExprEvaluator _evaluator;
	    private readonly EventBeanManufacturer _manufacturer;

	    public ContainedEventEvalArrayToEvent(ExprEvaluator evaluator, EventBeanManufacturer manufacturer)
        {
	        _evaluator = evaluator;
	        _manufacturer = manufacturer;
	    }

	    public object GetFragment(EventBean eventBean, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var result = _evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext)) as Array;
	        if (result == null) {
	            return null;
	        }

	        var events = new EventBean[result.Length];
	        for (int i = 0; i < events.Length; i++)
	        {
	            var column = result.GetValue(i);
	            if (column != null) {
	                events[i] = _manufacturer.Make(new object[] {column});
	            }
	        }
	        return events;
	    }
	}
} // end of namespace
