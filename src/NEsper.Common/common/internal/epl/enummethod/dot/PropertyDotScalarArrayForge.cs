///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotScalarArrayForge : ExprEnumerationEval,
        ExprEnumerationForge,
        ExprEnumerationGivenEvent,
        ExprEnumerationGivenEventForge,
        ExprNodeRenderable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _propertyName;
        private readonly int _streamId;
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _componentType;
        private readonly Type _getterReturnType;

        public PropertyDotScalarArrayForge(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter,
            Type componentType,
            Type getterReturnType)
        {
            _propertyName = propertyName;
            _streamId = streamId;
            _getter = getter;
            _componentType = componentType;
            _getterReturnType = getterReturnType;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventInQuestion = eventsPerStream[_streamId];
            return EvaluateEventGetROCollectionScalar(eventInQuestion, context);
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
        
        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refEPS = exprSymbol.GetAddEps(codegenMethodScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return CodegenEvaluateEventGetROCollectionScalar(
                ArrayAtIndex(refEPS, Constant(_streamId)),
                refExprEvalCtx,
                codegenMethodScope,
                codegenClassScope);
        }
        
        public Type ComponentTypeCollection => _componentType;

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }
        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprNodeRenderable ForgeRenderable => this;

        public ExprNodeRenderable EnumForgeRenderable => ForgeRenderable;

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }
     
        public ICollection<object> EvaluateEventGetROCollectionScalar(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            if (@event == null) {
                return null;
            }

            return EvaluateGetInternal(@event);
        }

        
        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public EventBean EvaluateEventGetEventBean(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }
        
        public CodegenExpression EvaluateEventGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope.MakeChild(
                typeof(ICollection<object>),
                typeof(PropertyDotScalarArrayForge),
                codegenClassScope);
            method
                .Block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .IfNullReturnNull(symbols.GetAddEvent(method))
                .MethodReturn(Unwrap<object>(
                    CodegenEvaluateGetInternal(symbols.GetAddEvent(method), codegenMethodScope, codegenClassScope)));
            return LocalMethod(method);
        }
        
        public CodegenExpression EvaluateEventGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
        
        public CodegenExpression CodegenEvaluateEventGetROCollectionScalar(
            CodegenExpression @event,
            CodegenExpression evalctx,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var componentType = _getterReturnType.GetComponentType();
            var collectionType = typeof(ICollection<>).MakeGenericType(componentType);

            var method = codegenMethodScope
                .MakeChild(collectionType, typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam<EventBean>("@event")
                .AddParam<ExprEvaluatorContext>("context")
                .Block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    CodegenEvaluateGetInternal(
                        Ref("@event"),
                        codegenMethodScope,
                        codegenClassScope));
            
            return LocalMethodBuild(method).Pass(@event).Pass(evalctx).Call();
        }

        private ICollection<object> EvaluateGetInternal(EventBean @event)
        {
            var value = _getter.Get(@event);
            if (value == null) {
                return null;
            }

            if (!value.GetType().IsArray) {
                Log.Warn(
                    "Expected array-type input from property '" + _propertyName + "' but received " + value.GetType());
                return null;
            }

            if (ComponentTypeCollection.IsValueType) {
                return value.Unwrap<object>(); // new ArrayWrappingCollection(value);
            }

            return value.Unwrap<object>();
        }

        private CodegenExpression CodegenEvaluateGetInternal(
            CodegenExpression @event,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var componentType = _getterReturnType.GetComponentType();
            var collectionType = typeof(ICollection<>).MakeGenericType(componentType);
            
            var block = codegenMethodScope
                .MakeChild(collectionType, typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam<EventBean>("@event")
                .Block
                .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                .DeclareVar(
                    _getterReturnType,
                    "value",
                    CodegenLegoCast.CastSafeFromObjectType(
                        _getterReturnType,
                        _getter.EventBeanGetCodegen(Ref("@event"), codegenMethodScope, codegenClassScope)))
                .IfRefNullReturnNull("value");
            
            CodegenMethod method;
            if (_componentType.CanNotBeNull() ||
                _componentType.GetUnboxedType().CanNotBeNull()) {
                method = block.MethodReturn(Unwrap(componentType, Ref("value")));
            }
            else {
                method = block.MethodReturn(Ref("value"));
            }

            return LocalMethodBuild(method).Pass(@event).Call();
        }
    }
} // end of namespace