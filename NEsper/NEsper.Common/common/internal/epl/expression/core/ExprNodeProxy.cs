///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.common.client;
using Castle.DynamicProxy;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeProxy : IInterceptor
    {
        private static readonly MethodInfo TARGET_GETFORGE;
        private static readonly MethodInfo TARGET_EQUALSNODE;
        private static readonly ProxyGenerator generator = new ProxyGenerator();

        static ExprNodeProxy()
        {
            TARGET_GETFORGE = typeof(ExprNode).GetMethod("GetForge");
            TARGET_EQUALSNODE = typeof(ExprNode).GetMethod("EqualsNode");
            if (TARGET_GETFORGE == null || TARGET_EQUALSNODE == null) {
                throw new EPRuntimeException("Failed to find required methods");
            }
        }

        public ExprNodeProxy(ExprNode exprNode)
        {
            Proxy = exprNode;
        }

        public ExprNode Proxy { get; }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == TARGET_EQUALSNODE) {
                HandleEqualsNode(invocation);
            }
            else if (invocation.Method == TARGET_GETFORGE) {
                HandleGetForge(invocation);
            }
            else {
                invocation.ReturnValue = invocation.Method.Invoke(
                    invocation.Proxy, invocation.Arguments);
            }
        }

        public static object NewInstance(ExprNode exprNode)
        {
            return (ExprForge) generator.CreateInterfaceProxyWithoutTarget(
                exprNode.GetType(),
                exprNode.GetType().GetInterfaces(),
                new ExprNodeProxy(exprNode));
        }

        private void HandleEqualsNode(IInvocation invocation)
        {
            var args = invocation.Arguments;

            ExprNode otherNode;
            try {
                otherNode = ((ExprNodeProxy) args[0]).Proxy;
            }
            catch (ArgumentException) {
                otherNode = (ExprNode) args[0];
            }

            invocation.ReturnValue = Proxy.EqualsNode(otherNode, (bool) args[1]);
        }

        private void HandleGetForge(IInvocation invocation)
        {
            var expressionToString = "undefined";
            try {
                expressionToString = ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(Proxy);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception) {
                // no action
            }

            var forge = (ExprForge) invocation.Method.Invoke(Proxy, invocation.Arguments);
            invocation.ReturnValue = ExprForgeProxy.NewInstance(expressionToString, forge);
        }
    }
} // end of namespace