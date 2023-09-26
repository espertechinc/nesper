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
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.streamtype;
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

        public Type ValueType { get; private set; }

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

        public Type EvaluationType => ValueType;

        public ExprNodeRenderable ExprForgeRenderable => this;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (ValueType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                EvaluationType,
                typeof(ExprContextPropertyNodeImpl),
                codegenClassScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            var block = methodNode.Block
                .DeclareVar<EventBean>("props", ExprDotName(refExprEvalCtx, "ContextProperties"))
                .IfRefNullReturnNull("props");
            block.MethodReturn(
                CodegenLegoCast.CastSafeFromObjectType(
                    ValueType,
                    getter.EventBeanGetCodegen(Ref("props"), methodNode, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                    GetType(),
                    this,
                    "ExprContextProp",
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope)
                .Build();
        }

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (validationContext.ContextDescriptor == null) {
                throw new ExprValidationException(
                    "Context property '" + PropertyName + "' cannot be used in the expression as provided");
            }

            var eventType = (EventTypeSPI)validationContext.ContextDescriptor.ContextPropertyRegistry.ContextEventType;
            if (eventType == null) {
                throw new ExprValidationException(
                    "Context property '" + PropertyName + "' cannot be used in the expression as provided");
            }

            getter = eventType.GetGetterSPI(PropertyName);
            var propertyType = eventType.GetPropertyType(PropertyName);
            if (getter == null || propertyType == null) {
                throw new ExprValidationException(
                    "Context property '" +
                    PropertyName +
                    "' is not a known property, known properties are " +
                    eventType.PropertyNames.RenderAny());
            }

            ValueType = eventType.GetPropertyType(PropertyName).GetBoxedType();
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write("context.");
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

            var that = (ExprContextPropertyNodeImpl)node;
            return PropertyName.Equals(that.PropertyName);
        }

        public ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor)
        {
            var eventType = (EventTypeSPI)contextDescriptor.ContextPropertyRegistry.ContextEventType;
            var fragmentEventType = eventType?.GetFragmentType(PropertyName);
            if (fragmentEventType == null || fragmentEventType.IsIndexed) {
                return null;
            }

            var forge = new ExprContextPropertyNodeFragmentEnumerationForge(
                PropertyName,
                fragmentEventType.FragmentType,
                getter);
            return new ExprEnumerationForgeDesc(forge, true, -1);
        }
    }
} // end of namespace