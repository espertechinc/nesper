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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprGroupingNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (!validationContext.IsAllowRollupFunctions) {
                throw ExprGroupingIdNode.MakeException("grouping");
            }

            return null;
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(int?);

        public override ExprForge Forge => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ExprNodeUtilityPrint.ToExpressionStringWFunctionName("grouping", ChildNodes, writer);
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.DEPLOYCONST;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
    }
} // end of namespace