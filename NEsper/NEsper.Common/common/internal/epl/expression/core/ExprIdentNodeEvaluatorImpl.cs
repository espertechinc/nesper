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

using com.espertech.esper.common.@internal.epl.dataflow.realize;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

using Constant = System.Reflection.Metadata.Constant;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprIdentNodeEvaluatorImpl : ExprIdentNodeEvaluator
    {
        private readonly int streamNum;
        private readonly EventPropertyGetterSPI propertyGetter;
        private readonly Type identType;
        private readonly Type returnType;
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
            this.identType = returnType;
            // Ident nodes when evaluated can return a value or null (if for example the underlying object is null).  Because of this,
            // we want to ensure that the return type is boxed so that we can always return null.  This means that primitives may need
            // to be unboxed elsewhere.
            this.returnType = returnType.GetBoxedType();
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
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return propertyGetter.Get(@event);
        }

        public Type GetCodegenReturnType(Type requiredType)
        {
            if (requiredType == typeof(object)) {
                return typeof(object);
            } else if (requiredType == returnType) {
                return returnType;
            } else if (requiredType.IsBoxedType() && requiredType == returnType) { // returnType is always boxed
                return requiredType;
            } else if (returnType == requiredType.GetBoxedType()) { // returnType is always boxed
                return requiredType; // they want the unboxed version, but we have the boxed... not sure
                //return returnType;
            }

            throw new ArgumentException(nameof(requiredType) + " and " + nameof(returnType) + " are incompatible");
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

            var targetType = GetCodegenReturnType(requiredType);
            var method = parent.MakeChild(targetType, this.GetType(), classScope);
            method.Block
                .DeclareVar(targetType, "value", CodegenGet(requiredType, method, symbols, classScope))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                        .Get("AuditProvider")
                        .Add(
                            "Property",
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

            var castTargetType = GetCodegenReturnType(requiredType);
            var useUnderlying = exprSymbol.IsAllowUnderlyingReferences &&
                                 !identNode.ResolvedPropertyName.Contains("?") &&
                                 !(eventType is WrapperEventType) &&
                                 !(eventType is VariantEventType);
            if (useUnderlying && !optionalEvent) {
                var underlying = exprSymbol.GetAddRequiredUnderlying(
                    codegenMethodScope,
                    streamNum,
                    eventType,
                    false);
                var property = propertyGetter.UnderlyingGetCodegen(underlying, codegenMethodScope, codegenClassScope);
                return CodegenLegoCast.CastSafeFromObjectType(castTargetType, property);
            }

            var method = codegenMethodScope.MakeChild(
                castTargetType, this.GetType(), codegenClassScope);
            var block = method.Block;

            if (useUnderlying) {
                var underlying = exprSymbol.GetAddRequiredUnderlying(
                    method,
                    streamNum,
                    eventType,
                    true);

                if (!requiredType.IsBoxedType()) {
#if THROW_VALUE_ON_NULL
                    block.IfRefNullThrowException(underlying);
#else
                    block.IfRefNull(underlying).MethodReturn(new CodegenExpressionDefault(requiredType));
#endif
                }
                else {
                    block.IfRefNullReturnNull(underlying);
                }

                block.MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        castTargetType,
                        propertyGetter.UnderlyingGetCodegen(underlying, method, codegenClassScope)));
            }
            else {
                var refEPS = exprSymbol.GetAddEPS(method);
                method.Block.DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(streamNum)));
                if (optionalEvent) {
                    if (!requiredType.IsBoxedType()) {
#if THROW_VALUE_ON_NULL
                        block.IfRefNullThrowException(Ref("@event"));
#else
                        block.IfRefNull(Ref("@event")).MethodReturn(new CodegenExpressionDefault(requiredType));
#endif
                    }
                    else {
                        block.IfRefNullReturnNull(Ref("@event"));
                    }
                }

                block.MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        castTargetType,
                        propertyGetter.EventBeanGetCodegen(@Ref("@event"), method, codegenClassScope)));
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
            var theEvent = eventsPerStream[streamNum];
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