///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.hook;

namespace com.espertech.esper.epl.expression.core
{
    [Serializable]
    public class ExprNodeUtilExprEvalMethodContext : ExprEvaluator
    {
        private readonly String _functionName;

        public ExprNodeUtilExprEvalMethodContext(String functionName)
        {
            _functionName = functionName;
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var context = evaluateParams.ExprEvaluatorContext;
            return new EPLMethodInvocationContext(
                context.StatementName,
                context.AgentInstanceId, 
                context.EngineURI, 
                _functionName,
                context.StatementUserObject
            );
        }

        public Type ReturnType
        {
            get { return typeof (EPLMethodInvocationContext); }
        }
    }
}
