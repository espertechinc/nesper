///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.hook;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprNodeUtilExprEvalMethodContext : ExprEvaluator
    {
        private readonly EPLMethodInvocationContext _defaultContextForFilters;

        public ExprNodeUtilExprEvalMethodContext(
            string engineURI,
            string functionName,
            EventBeanService eventBeanService)
        {
            _defaultContextForFilters = new EPLMethodInvocationContext(
                null, -1, engineURI, functionName, null, eventBeanService);
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            var context = evaluateParams.ExprEvaluatorContext;
            if (context == null)
            {
                return _defaultContextForFilters;
            }
            return new EPLMethodInvocationContext(
                context.StatementName,
                context.AgentInstanceId,
                _defaultContextForFilters.EngineURI,
                _defaultContextForFilters.FunctionName,
                context.StatementUserObject,
                _defaultContextForFilters.EventBeanService
                );
        }

        public Type ReturnType
        {
            get { return typeof (EPLMethodInvocationContext); }
        }
    }
} // end of namespace