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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprIdentNodeEvaluatorImpl : ExprIdentNodeEvaluator
    {
        private int _streamNum;
        private readonly EventPropertyGetterSPI _propertyGetter;
        private readonly Type _identType;
        private readonly Type _returnType;
        private readonly ExprIdentNode _identNode;
        private readonly EventTypeSPI _eventType;
        private bool _optionalEvent;
        private bool _audit;

        public ExprIdentNodeEvaluatorImpl(
            int streamNum,
            EventPropertyGetterSPI propertyGetter,
            Type returnType,
            ExprIdentNode identNode,
            EventTypeSPI eventType,
            bool optionalEvent,
            bool audit)
        {
            _streamNum = streamNum;
            _propertyGetter = propertyGetter;
            _identType = returnType;
            _returnType = returnType;
            _identNode = identNode;
            _eventType = eventType;
            _optionalEvent = optionalEvent;
            _audit = audit;
        }

        public bool OptionalEvent {
            get => _optionalEvent;
            set => _optionalEvent = value;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[_streamNum];
            if (@event == null) {
                return null;
            }

            return _propertyGetter.Get(@event);
        }

        public Type GetCodegenReturnType(Type requiredType)
        {
            if (requiredType == typeof(object)) {
                return typeof(object);
            }
            else if (requiredType == _returnType) {
                // Case: TX = TX
                return _returnType;
            }

            var requiredIsBoxed = requiredType.IsBoxedType();
            var returnIsBoxed = _returnType.IsBoxedType();

            if (requiredIsBoxed && requiredType == _returnType.GetBoxedType()) {
                // Case: TX? is requested, we have TX
                return requiredType;
            }
            else if (returnIsBoxed && _returnType == requiredType.GetBoxedType()) {
                // Case: TX is requested, we have TX?
                // they want the unboxed version, but we have the boxed... not sure,
                // we pass it along but will require the code to unbox the value.
                return requiredType;
            }

            // Alright, maybe we're dealing with type widening here.  Normally this is
            // validated before we get here and we're just dealing with boxing and unboxing
            // conditions.  Unfortunately, we're leaning very heavily on the "compiler"
            // doing the right thing rather than doing what want it to do.

            if (requiredIsBoxed) {
                return requiredType;
            }

            return _returnType;

            //throw new ArgumentException(nameof(requiredType) + " and " + nameof(returnType) + " are incompatible");
        }

        public CodegenExpression Codegen(
            Type requiredType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            if (!_audit) {
                return CodegenGet(requiredType, parent, symbols, classScope);
            }

            var targetType = GetCodegenReturnType(requiredType);
            var method = parent.MakeChild(targetType, GetType(), classScope);
            var valueInitializer = CodegenGet(requiredType, method, symbols, classScope);
            method.Block
                .DeclareVar(targetType, "value", valueInitializer)
                .Expression(
                    ExprDotMethodChain(symbols.GetAddExprEvalCtx(method))
                        .Get("AuditProvider")
                        .Add(
                            "Property",
                            Constant(_identNode.ResolvedPropertyName),
                            Ref("value"),
                            symbols.GetAddExprEvalCtx(method)))
                .MethodReturn(Ref("value"));
            return LocalMethod(method);
        }

        private CodegenExpression CodegenGet(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            if (_returnType == null) {
                return ConstantNull();
            }

            var castTargetType = GetCodegenReturnType(requiredType);
            var useUnderlying = exprSymbol.IsAllowUnderlyingReferences &&
                                !_identNode.ResolvedPropertyName.Contains("?") &&
                                !(_eventType is WrapperEventType) &&
                                !(_eventType is VariantEventType);
            if (useUnderlying && !_optionalEvent) {
                var underlying = exprSymbol.GetAddRequiredUnderlying(
                    codegenMethodScope,
                    _streamNum,
                    _eventType,
                    false);
                var property = _propertyGetter.UnderlyingGetCodegen(underlying, codegenMethodScope, codegenClassScope);
                return CodegenLegoCast.CastSafeFromObjectType(castTargetType, property);
            }

            var method = codegenMethodScope.MakeChild(
                castTargetType,
                GetType(),
                codegenClassScope);
            var block = method.Block;

            if (useUnderlying) {
                var underlying = exprSymbol.GetAddRequiredUnderlying(
                    method,
                    _streamNum,
                    _eventType,
                    true);

                if (castTargetType.CanNotBeNull()) {
#if THROW_VALUE_ON_NULL
                    block.IfRefNullThrowException(underlying);
#else
                    block
                        .IfNull(underlying)
                        .BlockReturn(new CodegenExpressionDefault(castTargetType));
#endif
                }
                else {
                    block.IfNullReturnNull(underlying);
                }

                block.MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        castTargetType,
                        _propertyGetter.UnderlyingGetCodegen(underlying, method, codegenClassScope)));
            }
            else {
                var refEPS = exprSymbol.GetAddEPS(method);
                method.Block.DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamNum)));
                if (_optionalEvent) {
                    if (castTargetType.CanNotBeNull()) {
#if THROW_VALUE_ON_NULL
                        block.IfRefNullThrowException(Ref("@event"));
#else
                        block
                            .IfNull(Ref("@event"))
                            .BlockReturn(new CodegenExpressionDefault(castTargetType));
#endif
                    }
                    else {
                        block.IfNullReturnNull(Ref("@event"));
                    }
                }

                block.MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        castTargetType,
                        _propertyGetter.EventBeanGetCodegen(Ref("@event"), method, codegenClassScope)));
            }

            return LocalMethod(method);
        }

        public Type EvaluationType => _returnType;

        public EventPropertyGetterSPI Getter => _propertyGetter;

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
            var theEvent = eventsPerStream[_streamNum];
            if (theEvent == null) {
                return false;
            }

            return _propertyGetter.IsExistsProperty(theEvent);
        }

        public int StreamNum => _streamNum;

        public bool IsContextEvaluated => false;

        public EventTypeSPI EventType => _eventType;
    }
} // end of namespace