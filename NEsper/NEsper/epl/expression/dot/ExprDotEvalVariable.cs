///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.enummethod.dot;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.variable;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.dot
{
    public class ExprDotEvalVariable : ExprEvaluator
    {
        private readonly ExprDotNode _dotNode;
        private readonly VariableReader _variableReader;
        private readonly ExprDotStaticMethodWrap _resultWrapLambda;
        private readonly ExprDotEval[] _chainEval;
    
        public ExprDotEvalVariable(ExprDotNode dotNode, VariableReader variableReader, ExprDotStaticMethodWrap resultWrapLambda, ExprDotEval[] chainEval)
        {
            _dotNode = dotNode;
            _variableReader = variableReader;
            _resultWrapLambda = resultWrapLambda;
            _chainEval = chainEval;
        }

        public Type ReturnType
        {
            get
            {
                if (_chainEval.Length == 0)
                {
                    return _variableReader.VariableMetaData.VariableType;
                }
                else
                {
                    return _chainEval[_chainEval.Length - 1].TypeInfo.GetClassSingleValued();
                }
            }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprDot(_dotNode);}
    
            Object result = _variableReader.Value;
            result = ExprDotNodeUtility.EvaluateChainWithWrap(
                _resultWrapLambda, result, 
                _variableReader.VariableMetaData.EventType, 
                _variableReader.VariableMetaData.VariableType, _chainEval, 
                evaluateParams.EventsPerStream, 
                evaluateParams.IsNewData, 
                evaluateParams.ExprEvaluatorContext);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprDot(result);}
            return result;
        }
    }
}
