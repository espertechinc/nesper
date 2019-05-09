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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Represents an stream property identifier in a filter expressiun tree.
    /// </summary>
    public class ExprContextPropertyNodeImpl : ExprNodeBase,
        ExprContextPropertyNode,
        ExprEvaluator,
        ExprForgeInstrumentable
    {
        [NonSerialized] private EventPropertyGetterSPI getter;

        public ExprContextPropertyNodeImpl(string propertyName)
        {
            PropertyName = propertyName;
        }

        public override ExprForge Forge => this;

        public bool IsConstantResult => false;

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public string PropertyName { get; }

        public Type Type { get; private set; }

        public EventPropertyGetterSPI Getter => getter;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var props = context.ContextProperties;
            var result = props != null ? getter.Get(props) : null;
            return result;
        }

        public ExprEvaluator ExprEvaluator => this;

        public Type EvaluationType => Type;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                EvaluationType, typeof(ExprContextPropertyNodeImpl), codegenClassScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            var block = methodNode.Block
                .DeclareVar(typeof(EventBean), "props", ExprDotMethod(refExprEvalCtx, "getContextProperties"))
                .IfRefNullReturnNull("props");
            block.MethodReturn(
                CodegenLegoCast.CastSafeFromObjectType(
                    Type, getter.EventBeanGetCodegen(Ref("props"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(), this, "ExprContextProp", requiredType, codegenMethodScope, exprSymbol, codegenClassScope)
                .Build();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (validationContext.ContextDescriptor == null) {
                throw new ExprValidationException(
                    "Context property '" + PropertyName + "' cannot be used in the expression as provided");
            }

            var eventType = (EventTypeSPI) validationContext.ContextDescriptor.ContextPropertyRegistry.ContextEventType;
            if (eventType == null) {
                throw new ExprValidationException(
                    "Context property '" + PropertyName + "' cannot be used in the expression as provided");
            }

            getter = eventType.GetGetterSPI(PropertyName);
            if (getter == null) {
                throw new ExprValidationException(
                    "Context property '" + PropertyName + "' is not a known property, known properties are " +
                    eventType.PropertyNames.RenderAny());
            }

            Type = eventType.GetPropertyType(PropertyName).GetBoxedType();
            return null;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(PropertyName);
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (this == node) {
                return true;
            }

            if (node == null || GetType() != node.GetType()) {
                return false;
            }

            var that = (ExprContextPropertyNodeImpl) node;
            return PropertyName.Equals(that.PropertyName);
        }
    }
} // end of namespace