///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.supportunit.util
{
    public class SupportBoolExprNode : ExprNodeBase,
        ExprForge,
        ExprEvaluator
    {
        private readonly bool evaluateResult;

        public SupportBoolExprNode(bool evaluateResult)
        {
            this.evaluateResult = evaluateResult;
        }

        public bool IsConstantResult => false;

        public override ExprForge Forge => this;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprNodeRenderable ForgeRenderable => this;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return evaluateResult;
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

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public Type EvaluationType => typeof(bool?);

        public ExprNodeRenderable ExprForgeRenderable => ForgeRenderable;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            throw new UnsupportedOperationException("not implemented");
        }
    }
} // end of namespace
