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
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.type;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Expression for use within crontab to specify a wildcard.
    /// </summary>
    public class ExprWildcardImpl : ExprNodeBase,
        ExprForge,
        ExprEvaluator,
        ExprWildcard
    {
        private EventType _eventType;

        public ExprWildcardImpl()
        {
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("*");
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public ExprEvaluator ExprEvaluator => this;

        public bool IsConstantResult => true;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            return node is ExprWildcardImpl;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (validationContext.StreamTypeService.EventTypes.Length > 0) {
                _eventType = validationContext.StreamTypeService.EventTypes[0];
            }

            return null;
        }

        public override ExprForge Forge => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

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
            return EnumValue(typeof(WildcardParameter), "Instance");
        }

        public Type EvaluationType => typeof(WildcardParameter);


        public ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor)
        {
            if (_eventType == null) {
                return null;
            }

            if (streamTypeService.EventTypes.Length > 1) {
                return null;
            }

            return new ExprEnumerationForgeDesc(
                new ExprStreamUnderlyingNodeEnumerationForge("*", 0, _eventType),
                streamTypeService.IStreamOnly[0],
                0);
        }
    }
} // end of namespace