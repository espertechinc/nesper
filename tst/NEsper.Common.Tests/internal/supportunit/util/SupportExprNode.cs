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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportExprNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private static int validateCount;

        private object value;

        public SupportExprNode(Type type)
        {
            EvaluationType = type;
            value = null;
        }

        public SupportExprNode(object value)
        {
            EvaluationType = value.GetType();
            this.value = value;
        }

        public SupportExprNode(
            object value,
            Type type)
        {
            this.value = value;
            EvaluationType = type;
        }

        public override ExprForge Forge => this;

        public bool IsConstantResult => false;

        public int ValidateCountSnapshot { get; private set; }

        public ExprNodeRenderable ForgeRenderable => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public static int ValidateCount
        {
            set => validateCount = value;
        }

        public object Value
        {
            set => this.value = value;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return value;
        }

        public ExprEvaluator ExprEvaluator => this;

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public Type EvaluationType { get; }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Keep a count for if and when this was validated
            validateCount++;
            ValidateCountSnapshot = validateCount;
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            if (value is string)
            {
                writer.Write("\"" + value + "\"");
            }
            else if (value == null)
            {
                writer.Write("null");
            }
            else
            {
                value.RenderAny(writer);
            }
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is SupportExprNode))
            {
                return false;
            }

            var other = (SupportExprNode) node;
            return value.Equals(other.value);
        }
    }
} // end of namespace
