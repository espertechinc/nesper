///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.supportregression.client
{
    public class SupportAggMFStatePlainScalar : AggregationState
    {
        private readonly SupportAggMFStatePlainScalarFactory _factory;
    
        private Object _lastValue;
    
        public SupportAggMFStatePlainScalar(SupportAggMFStatePlainScalarFactory factory) 
        {
            _factory = factory;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            _lastValue = _factory.Evaluator.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // ever semantics
        }
    
        public void Clear()
        {
            _lastValue = null;
        }

        public int Count
        {
            get { return _lastValue == null ? 0 : 1; }
        }

        public object LastValue
        {
            get { return _lastValue; }
        }
    }
}
