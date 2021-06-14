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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprGroupingIdNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        public int Id { get; set; }

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return Id;
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.COMPILETIMECONST;

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => typeof(int?);

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return Constant(Id);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (!validationContext.IsAllowRollupFunctions) {
                throw MakeException("grouping_id");
            }

            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ExprNodeUtilityPrint.ToExpressionStringWFunctionName("grouping_id", ChildNodes, writer);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return false;
        }

        public static ExprValidationException MakeException(string functionName)
        {
            return new ExprValidationException(
                "The " +
                functionName +
                " function requires the group-by clause to specify rollup, cube or grouping sets, and may only be used in the select-clause, having-clause or order-by-clause");
        }
    }
} // end of namespace