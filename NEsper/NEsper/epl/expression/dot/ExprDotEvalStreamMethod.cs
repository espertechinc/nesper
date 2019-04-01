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
using com.espertech.esper.epl.rettype;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEvalStreamMethod : ExprEvaluator
    {
        private readonly ExprDotNode _dotNode;
        private readonly int _streamNumber;
        private readonly ExprDotEval[] _evaluators;
    
        public ExprDotEvalStreamMethod(ExprDotNode dotNode, int streamNumber, ExprDotEval[] evaluators)
        {
            _dotNode = dotNode;
            _streamNumber = streamNumber;
            _evaluators = evaluators;
        }

        public Type ReturnType
        {
            get
            {
                return EPTypeHelper.GetNormalizedClass(_evaluators[_evaluators.Length - 1].TypeInfo);
            }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprStreamUndMethod(_dotNode);}
    
            // get underlying event
            EventBean theEvent = evaluateParams.EventsPerStream[_streamNumber];
            if (theEvent == null) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamUndMethod(null);}
                return null;
            }
            Object inner = theEvent.Underlying;
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDotChain(EPTypeHelper.SingleValue(theEvent.EventType.UnderlyingType), inner, _evaluators);}
            inner = ExprDotNodeUtility.EvaluateChain(_evaluators, inner, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDotChain();}
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamUndMethod(inner);}
            return inner;
        }
    }
}
