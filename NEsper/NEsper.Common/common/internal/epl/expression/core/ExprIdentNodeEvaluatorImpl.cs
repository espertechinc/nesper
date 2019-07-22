///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;

using System;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprIdentNodeEvaluatorImpl : ExprIdentNodeEvaluator
    {
        private readonly int streamNum;
        private readonly EventPropertyGetterSPI propertyGetter;
        internal readonly Type returnType;
        private readonly ExprIdentNode identNode;
        private readonly EventType eventType;
        private bool optionalEvent;
        private bool audit;

        public ExprIdentNodeEvaluatorImpl(
            int streamNum,
            EventPropertyGetterSPI propertyGetter,
            Type returnType,
            ExprIdentNode identNode,
            EventType eventType,
            bool optionalEvent,
            bool audit)
        {
            this.streamNum = streamNum;
            this.propertyGetter = propertyGetter;
            this.returnType = returnType;
            this.identNode = identNode;
            this.eventType = eventType;
            this.optionalEvent = optionalEvent;
            this.audit = audit;
        }

        public bool OptionalEvent {
            set { this.optionalEvent = value; }
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return propertyGetter.Get(@event);
        }

        public Type GetCodegenReturnType(Type requiredType)
        {
            return requiredType == typeof(object) ? typeof(object) : returnType;
        }

        public CodegenExpression Codegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (!audit) {
                return CodegenGet(requiredType, parent, symbols, classScope);
            }

            Type targetType = GetCodegenReturnType(requiredType);
            CodegenMethod method = parent.MakeChild(targetType, this.GetType(), classScope);
            method.Block
                .DeclareVar(targetType, "value", CodegenGet(requiredType, method, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                        .Add("getAuditProvider")
                        .Add(
                            "property",
                            Constant(identNode.ResolvedPropertyName),
                            @Ref("value"),
                            symbols.GetAddExprEvalCtx(method)))
                .MethodReturn(@Ref("value"));
            return LocalMethod(method);
        }

        private CodegenExpression CodegenGet(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (returnType == null) {
                return ConstantNull();
            }

            Type castTargetType = GetCodegenReturnType(requiredType);
            bool useUnderlying = exprSymbol.IsAllowUnderlyingReferences &&
                                 !identNode.ResolvedPropertyName.Contains("?") &&
                                 !(eventType is WrapperEventType) &&
                                 !(eventType is VariantEventType);
            if (useUnderlying && !optionalEvent) {
                CodegenExpressionRef underlying = exprSymbol.GetAddRequiredUnderlying(
                    codegenMethodScope,
                    streamNum,
                    eventType,
                    false);
                return CodegenLegoCast.CastSafeFromObjectType(
                    castTargetType,
                    propertyGetter.UnderlyingGetCodegen(underlying, codegenMethodScope, codegenClassScope));
            }

            CodegenMethod method = codegenMethodScope.MakeChild(castTargetType, this.GetType(), codegenClassScope);
            CodegenBlock block = method.Block;

            if (useUnderlying) {
                CodegenExpressionRef underlying = exprSymbol.GetAddRequiredUnderlying(
                    method,
                    streamNum,
                    eventType,
                    true);
                block.IfRefNullReturnNull(underlying)
                    .MethodReturn(
                        CodegenLegoCast.CastSafeFromObjectType(
                            castTargetType,
                            propertyGetter.UnderlyingGetCodegen(underlying, method, codegenClassScope)));
            }
            else {
                CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(method);
                method.Block.DeclareVar<EventBean>("event", ArrayAtIndex(refEPS, Constant(streamNum)));
                if (optionalEvent) {
                    block.IfRefNullReturnNull("event");
                }

                block.MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        castTargetType,
                        propertyGetter.EventBeanGetCodegen(@Ref("event"), method, codegenClassScope)));
            }

            return LocalMethod(method);
        }

        public Type EvaluationType {
            get => returnType;
        }

        public EventPropertyGetterSPI Getter {
            get => propertyGetter;
        }

        /// <summary>
        /// Returns true if the property exists, or false if not.
        /// </summary>
        /// <param name="eventsPerStream">each stream's events</param>
        /// <param name="isNewData">if the stream represents insert or remove stream</param>
        /// <returns>true if the property exists, false if not</returns>
        public bool EvaluatePropertyExists(
            EventBean[] eventsPerStream,
            bool isNewData)
        {
            EventBean theEvent = eventsPerStream[streamNum];
            if (theEvent == null) {
                return false;
            }

            return propertyGetter.IsExistsProperty(theEvent);
        }

        public int StreamNum {
            get => streamNum;
        }

        public bool IsContextEvaluated {
            get => false;
        }
    }
} // end of namespace