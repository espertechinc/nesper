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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprForgeProxy : IInterceptor
    {
        private static readonly MethodInfo TARGET_EVALUATECODEGEN;

        private readonly string expressionToString;
        private readonly ExprForge forge;

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
            this.expressionToString = expressionToString;
            this.forge = forge;
        }

        /// <summary>
        ///     Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == TARGET_EVALUATECODEGEN) {
                var args = invocation.Arguments;
                var evaluationType = forge.EvaluationType;
                var requiredType = (Type) args[args.Length - 4];
                var parent = (CodegenMethodScope) args[args.Length - 3];
                var symbols = (ExprForgeCodegenSymbol) args[args.Length - 2];
                var codegenClassScope = (CodegenClassScope) args[args.Length - 1];

                if (evaluationType == null) {
                    invocation.ReturnValue = forge.EvaluateCodegen(requiredType, parent, symbols, codegenClassScope);
                    return;
                }

                var method = parent.MakeChild(evaluationType, typeof(ExprForgeProxy), codegenClassScope);
                if (evaluationType == typeof(void)) {
                    method.Block.Expression(forge.EvaluateCodegen(requiredType, method, symbols, codegenClassScope))
                        .Expression(
                            ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                                .Get("AuditProvider")
                                .Add(
                                    "expression",
                                    Constant(expressionToString),
                                    Constant("(void)"),
                                    symbols.GetAddExprEvalCtx(method)))
                        .MethodEnd();
                }
                else {
                    method.Block.DeclareVar(
                            evaluationType,
                            "result",
                            forge.EvaluateCodegen(evaluationType, method, symbols, codegenClassScope))
                        .Expression(
                            ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                                .Get("AuditProvider")
                                .Add(
                                    "expression",
                                    Constant(expressionToString),
                                    Ref("result"),
                                    symbols.GetAddExprEvalCtx(method)))
                        .MethodReturn(Ref("result"));
                }

                invocation.ReturnValue = LocalMethod(method);
                return;
            }

            invocation.ReturnValue = invocation.Method.Invoke(forge, invocation.Arguments);
        }

        public static ExprForge NewInstance(
            string expressionToString,
            ExprForge forge)
        {
            var generator = new ProxyGenerator();
            return (ExprForge) generator.CreateInterfaceProxyWithoutTarget(
                forge.GetType(),
                forge.GetType().GetInterfaces(),
                new ExprForgeProxy(expressionToString, forge));
        }
    }
} // end of namespace