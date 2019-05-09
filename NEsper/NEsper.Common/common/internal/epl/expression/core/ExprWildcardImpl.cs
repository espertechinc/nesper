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
using com.espertech.esper.common.@internal.type;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Expression for use within crontab to specify a wildcard.
    /// </summary>
    [Serializable]
    public class ExprWildcardImpl : ExprNodeBase,
        ExprForge,
        ExprEvaluator,
        ExprWildcard
    {
        public ExprWildcardImpl()
        {
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("*");
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.UNARY;
        }

        public ExprEvaluator ExprEvaluator {
            get => this;
        }

        public bool IsConstantResult {
            get => true;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprWildcardImpl;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            return null;
        }

        public override ExprForge Forge {
            get => this;
        }

        public ExprNodeRenderable ExprForgeRenderable {
            get => this;
        }

        public ExprForgeConstantType ForgeConstantType {
            get => ExprForgeConstantType.NONCONST;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return WildcardParameter.Instance;
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return EnumValue(typeof(WildcardParameter), "INSTANCE");
        }

        public Type EvaluationType {
            get => typeof(WildcardParameter);
        }
    }
} // end of namespace