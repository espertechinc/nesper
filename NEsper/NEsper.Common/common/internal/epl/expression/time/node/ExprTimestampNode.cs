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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.time.node
{
    /// <summary>
    /// Represents the CURRENT_TIMESTAMP() function or reserved keyword in an expression tree.
    /// </summary>
    public class ExprTimestampNode : ExprNodeBase,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        public ExprTimestampNode()
        {
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public Type EvaluationType {
            get => typeof(long?);
        }

        public override ExprForge Forge {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                this.GetType(),
                this,
                "ExprTimestamp",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionRef refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return ExprDotMethodChain(refExprEvalCtx).Get("TimeProvider").Get("Time");
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (this.ChildNodes.Length != 0) {
                throw new ExprValidationException("current_timestamp function node cannot have a child node");
            }

            return null;
        }

        public bool IsConstantResult {
            get => false;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("current_timestamp()");
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprTimestampNode)) {
                return false;
            }

            return true;
        }
    }
} // end of namespace