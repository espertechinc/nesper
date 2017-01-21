///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.named
{
    public abstract class NamedWindowOnMergeAction
    {
        private readonly ExprEvaluator _optionalFilter;
    
        protected NamedWindowOnMergeAction(ExprEvaluator optionalFilter)
        {
            _optionalFilter = optionalFilter;
        }
    
        public bool IsApplies(EventBean[] eventsPerStream, ExprEvaluatorContext context)
        {
            if (_optionalFilter == null)
            {
                return true;
            }
            var evaluateParams = new EvaluateParams(eventsPerStream, true, context);
            var result = _optionalFilter.Evaluate(evaluateParams);
            return result != null && (bool) result;
        }
    
        public abstract void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, OneEventCollection newData, OneEventCollection oldData, ExprEvaluatorContext exprEvaluatorContext);
        public abstract String GetName();
    }
}
