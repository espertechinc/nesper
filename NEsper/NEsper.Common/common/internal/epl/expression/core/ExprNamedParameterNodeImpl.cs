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
    [Serializable]
    public class ExprNamedParameterNodeImpl : ExprNodeBase,
        ExprNamedParameterNode,
        ExprForge,
        ExprEvaluator
    {
        public ExprNamedParameterNodeImpl(string parameterName)
        {
            ParameterName = parameterName;
        }

        public override ExprForge Forge => this;

        public ExprNode ForgeRenderable => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public bool IsConstantResult => false;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => null;

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public string ParameterName { get; }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(ParameterName);
            writer.Write(":");
            if (ChildNodes.Length > 1) {
                writer.Write("(");
            }

            ExprNodeUtilityPrint.ToExpressionStringParameterList(ChildNodes, writer);
            if (ChildNodes.Length > 1) {
                writer.Write(")");
            }
        }

        public override bool EqualsNode(
            ExprNode other,
            bool ignoreStreamPrefix)
        {
            if (!(other is ExprNamedParameterNode)) {
                return false;
            }

            var otherNamed = (ExprNamedParameterNode) other;
            return otherNamed.ParameterName.Equals(ParameterName);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }
    }
} // end of namespace