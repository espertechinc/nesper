///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using Castle.DynamicProxy;

namespace com.espertech.esper.epl.expression.core
{
    public class ExprNodeProxy : IInterceptor
    {
        private static readonly MethodInfo ExprEvaluatorGetterMethod =
            typeof(ExprNode).GetProperty("ExprEvaluator").GetGetMethod();

        private static readonly Assembly CastleAssembly =
            typeof (ProxyGenerator).Assembly;

        private readonly String _engineURI;
        private readonly String _statementName;
        private readonly ExprNode _exprNode;

        public static Object NewInstance(String engineURI, String statementName, ExprNode exprNode)
        {
            var generator = new ProxyGenerator();
            var interfaces = exprNode.GetType()
                .GetInterfaces()
                .Where(inf => inf.Assembly != CastleAssembly)
                .ToArray();

            return generator.CreateInterfaceProxyWithoutTarget(
                typeof(ExprNode), interfaces,
                new ExprNodeProxy(engineURI, statementName, exprNode));
        }

        /// <summary>
        /// Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method != ExprEvaluatorGetterMethod)
            {
                invocation.ReturnValue = invocation.Method.Invoke(_exprNode, invocation.Arguments);
            }
            else
            {
                String expressionToString = "undefined";
                try
                {
                    expressionToString = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_exprNode);
                }
                catch
                {
                    // no action
                }

                var evaluator = (ExprEvaluator)invocation.Method.Invoke(_exprNode, invocation.Arguments);
                invocation.ReturnValue = ExprEvaluatorProxy.NewInstance(
                    _engineURI, _statementName, expressionToString, evaluator);
            }
        }

        public ExprNodeProxy(String engineURI, String statementName, ExprNode exprNode)
        {
            _engineURI = engineURI;
            _statementName = statementName;
            _exprNode = exprNode;
        }
    }
}
