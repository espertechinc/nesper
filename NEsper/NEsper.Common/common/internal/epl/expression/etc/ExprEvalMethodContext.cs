///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.etc
{
    public class ExprEvalMethodContext : ExprForge,
        ExprEvaluator,
        ExprNodeRenderable
    {
        private readonly string functionName;

        public ExprEvalMethodContext(string functionName)
        {
            this.functionName = functionName;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (context == null) {
                return new EPLMethodInvocationContext(
                    null,
                    -1,
                    null,
                    functionName,
                    null,
                    null);
            }

            return new EPLMethodInvocationContext(
                context.StatementName,
                context.AgentInstanceId,
                context.RuntimeURI,
                functionName,
                context.UserObjectCompileTime,
                null);
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EPLMethodInvocationContext),
                typeof(ExprEvalMethodContext),
                codegenClassScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            var stmtName = ExprDotName(refExprEvalCtx, "StatementName");
            var cpid = ExprDotName(refExprEvalCtx, "AgentInstanceId");
            var runtimeURI = ExprDotName(refExprEvalCtx, "RuntimeURI");
            var userObject = ExprDotName(refExprEvalCtx, "UserObjectCompileTime");
            var eventBeanSvc = ExprDotName(refExprEvalCtx, "EventBeanService");
            methodNode.Block
                .IfCondition(EqualsNull(refExprEvalCtx))
                .BlockReturn(
                    NewInstance<EPLMethodInvocationContext>(
                        ConstantNull(),
                        Constant(-1),
                        ConstantNull(),
                        Constant(functionName),
                        ConstantNull(),
                        ConstantNull()))
                .MethodReturn(
                    NewInstance<EPLMethodInvocationContext>(
                        stmtName,
                        cpid,
                        runtimeURI,
                        Constant(functionName),
                        userObject,
                        eventBeanSvc));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType => typeof(EPLMethodInvocationContext);

        public ExprNodeRenderable ExprForgeRenderable => this;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(typeof(ExprEvalMethodContext).Name);
        }
    }
} // end of namespace