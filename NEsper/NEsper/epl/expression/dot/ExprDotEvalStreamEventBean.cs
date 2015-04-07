///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ExprDotEvalStreamEventBean : ExprEvaluator
    {
        private readonly ExprDotNode _exprDotNode;
        private readonly int _streamNumber;
        private readonly ExprDotEval[] _evaluators;
    
        public ExprDotEvalStreamEventBean(ExprDotNode exprDotNode, int streamNumber, ExprDotEval[] evaluators)
        {
            _exprDotNode = exprDotNode;
            _streamNumber = streamNumber;
            _evaluators = evaluators;
        }

        public Type ReturnType
        {
            get
            {
                return EPTypeHelper.GetClassSingleValued(_evaluators[_evaluators.Length - 1].TypeInfo);
            }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprStreamEventMethod(_exprDotNode);}
    
            EventBean theEvent = evaluateParams.EventsPerStream[_streamNumber];
            if (theEvent == null) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamEventMethod(null);}
                return null;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDotChain(EPTypeHelper.SingleEvent(theEvent.EventType), theEvent, _evaluators); }
            Object inner = ExprDotNodeUtility.EvaluateChain(_evaluators, theEvent, evaluateParams.EventsPerStream, evaluateParams.IsNewData, evaluateParams.ExprEvaluatorContext);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDotChain();}
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprStreamEventMethod(inner);}
            return inner;
        }
    }
}
