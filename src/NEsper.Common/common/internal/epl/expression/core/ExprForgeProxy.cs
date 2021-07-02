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

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprForgeProxy : IInterceptor
    {
        private static readonly MethodInfo TargetEvaluateCodegen;
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        private readonly string _expressionToString;
        private readonly ExprForge _forge;

        static ExprForgeProxy()
        {
            TargetEvaluateCodegen = typeof(ExprForge).GetMethod("EvaluateCodegen");
            if (TargetEvaluateCodegen == null) {
                throw new EPRuntimeException("Failed to find required methods");
            }
        }

        public ExprForgeProxy(
            string expressionToString,
            ExprForge forge)
        {
            this._expressionToString = expressionToString;
            this._forge = forge;
        }

        /// <summary>
        ///     Intercepts the specified invocation.
        /// </summary>
        /// <param name="invocation">The invocation.</param>
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method == TargetEvaluateCodegen) {
                var args = invocation.Arguments;
                var evaluationType = _forge.EvaluationType;
                var requiredType = (Type) args[args.Length - 4];
                var parent = (CodegenMethodScope) args[args.Length - 3];
                var symbols = (ExprForgeCodegenSymbol) args[args.Length - 2];
                var codegenClassScope = (CodegenClassScope) args[args.Length - 1];

                if (evaluationType.IsNullTypeSafe()) {
                    invocation.ReturnValue = _forge.EvaluateCodegen(requiredType, parent, symbols, codegenClassScope);
                    return;
                }

                var method = parent.MakeChild(evaluationType, typeof(ExprForgeProxy), codegenClassScope);
                if (evaluationType.IsVoid()) {
                    method.Block
                        .Expression(_forge.EvaluateCodegen(requiredType, method, symbols, codegenClassScope))
                        .DebugStack()
                        .Expression(
                            ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                                .Get("AuditProvider")
                                .Add("Expression", Constant(_expressionToString), Constant("(void)"), symbols.GetAddExprEvalCtx(method)))
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
                                .Add("Expression", Constant(_expressionToString), Ref("result"), symbols.GetAddExprEvalCtx(method)))
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
            return Generator.CreateInterfaceProxyWithTarget<ExprForge>(
                forge, new ExprForgeProxy(expressionToString, forge));
        }
    }
} // end of namespace