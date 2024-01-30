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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using Castle.DynamicProxy;

using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprForgeProxy : IInterceptor
    {
        private static readonly MethodInfo TARGET_EVALUATECODEGEN;
        private static readonly ProxyGenerator generator = new ProxyGenerator();

        private readonly string _expressionToString;
        private readonly ExprForge _forge;

        static ExprForgeProxy()
        {
            TARGET_EVALUATECODEGEN = typeof(ExprForge).GetMethod("EvaluateCodegen");
            if (TARGET_EVALUATECODEGEN == null) {
                throw new EPRuntimeException("Failed to find required methods");
            }
        }

        public ExprForgeProxy(
            string expressionToString,
            ExprForge forge)
        {
            _expressionToString = expressionToString;
            _forge = forge;
        }

        /// <summary>
        ///     Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == TARGET_EVALUATECODEGEN) {
                var args = invocation.Arguments;
                var evaluationType = _forge.EvaluationType;
                var requiredType = (Type)args[^4];
                var parent = (CodegenMethodScope)args[^3];
                var symbols = (ExprForgeCodegenSymbol)args[^2];
                var codegenClassScope = (CodegenClassScope)args[^1];

                if (evaluationType == null) {
                    invocation.ReturnValue = _forge.EvaluateCodegen(requiredType, parent, symbols, codegenClassScope);
                    return;
                }

                var method = parent.MakeChild(evaluationType, typeof(ExprForgeProxy), codegenClassScope);
                if (evaluationType.IsTypeVoid()) {
                    method.Block
                        .Expression(_forge.EvaluateCodegen(requiredType, method, symbols, codegenClassScope))
                        .DebugStack()
                        .Expression(
                            ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                                .Get("AuditProvider")
                                .Add(
                                    "Expression",
                                    Constant(_expressionToString),
                                    Constant("(void)"),
                                    symbols.GetAddExprEvalCtx(method)))
                        .MethodEnd();
                }
                else {
                    method.Block
                        .DebugStack()
                        .DeclareVar(
                            evaluationType,
                            "result",
                            _forge.EvaluateCodegen(evaluationType, method, symbols, codegenClassScope))
                        .Expression(
                            ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                                .Get("AuditProvider")
                                .Add(
                                    "Expression",
                                    Constant(_expressionToString),
                                    Ref("result"),
                                    symbols.GetAddExprEvalCtx(method)))
                        .MethodReturn(Ref("result"));
                }

                invocation.ReturnValue = LocalMethod(method);
                return;
            }

            invocation.ReturnValue = invocation.Method.Invoke(_forge, invocation.Arguments);
        }

        public static ExprForge NewInstance(
            string expressionToString,
            ExprForge forge)
        {
            return generator.CreateInterfaceProxyWithTarget<ExprForge>(
                forge,
                new ExprForgeProxy(expressionToString, forge));
        }
    }
} // end of namespace