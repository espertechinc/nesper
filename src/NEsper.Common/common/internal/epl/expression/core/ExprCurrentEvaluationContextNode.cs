///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Represents the "current_evaluation_context" function in an expression tree.
    /// </summary>
    public class ExprCurrentEvaluationContextNode : ExprNodeBase,
        ExprEvaluator,
        ExprForge
    {
        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return ExprCurrentEvaluationContextMake(exprEvaluatorContext);
        }

        public ExprEvaluator ExprEvaluator => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return StaticMethod(
                typeof(ExprCurrentEvaluationContextNode),
                "ExprCurrentEvaluationContextMake",
                refExprEvalCtx);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(EPLExpressionEvaluationContext);

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 0) {
                throw new ExprValidationException("current_evaluation_context function node cannot have a child node");
            }

            return null;
        }

        public bool IsConstantResult => false;

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="exprEvaluatorContext">ctx</param>
        /// <returns>wrapper</returns>
        public static EPLExpressionEvaluationContext ExprCurrentEvaluationContextMake(
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return new EPLExpressionEvaluationContext(
                exprEvaluatorContext.StatementName,
                exprEvaluatorContext.AgentInstanceId,
                exprEvaluatorContext.RuntimeURI,
                exprEvaluatorContext.UserObjectCompileTime);
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("current_evaluation_context()");
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprCurrentEvaluationContextNode;
        }
    }
} // end of namespace