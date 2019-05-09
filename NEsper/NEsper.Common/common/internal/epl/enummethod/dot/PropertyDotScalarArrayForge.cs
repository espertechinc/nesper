///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
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
        private readonly EventPropertyGetterSPI getter;
        private readonly Type getterReturnType;

        private readonly string propertyName;
        private readonly int streamId;

        public PropertyDotScalarArrayForge(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter,
            Type componentType,
            Type getterReturnType)
        {
            this.propertyName = propertyName;
            this.streamId = streamId;
            this.getter = getter;
            ComponentTypeCollection = componentType;
            this.getterReturnType = getterReturnType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventInQuestion = eventsPerStream[streamId];
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

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refEPS = exprSymbol.GetAddEPS(codegenMethodScope);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(codegenMethodScope);
            return CodegenEvaluateEventGetROCollectionScalar(
                ArrayAtIndex(refEPS, Constant(streamId)), refExprEvalCtx, codegenMethodScope, codegenClassScope);
        }

        public Type ComponentTypeCollection { get; }

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

        public ExprNodeRenderable EnumForgeRenderable => this;

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
                typeof(ICollection<object>), typeof(PropertyDotScalarArrayForge), codegenClassScope);
            method.Block
                .IfRefNullReturnNull(symbols.GetAddEvent(method))
                .MethodReturn(
                    CodegenEvaluateGetInternal(symbols.GetAddEvent(method), codegenMethodScope, codegenClassScope));
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
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().Name);
        }

        public CodegenExpression CodegenEvaluateEventGetROCollectionScalar(
            CodegenExpression @event,
            CodegenExpression evalctx,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), "context").Block
                .IfRefNullReturnNull("event")
                .MethodReturn(CodegenEvaluateGetInternal(Ref("event"), codegenMethodScope, codegenClassScope));
            return LocalMethodBuild(method).Pass(@event).Pass(evalctx).Call();
        }

        private ICollection<object> EvaluateGetInternal(EventBean @event)
        {
            var value = getter.Get(@event);
            if (value == null) {
                return null;
            }

            if (!value.GetType().IsArray) {
                Log.Warn(
                    "Expected array-type input from property '" + propertyName + "' but received " + value.GetType());
                return null;
            }

            if (ComponentTypeCollection.IsPrimitive) {
                return value.Unwrap<object>(); // new ArrayWrappingCollection(value);
            }

            return value.Unwrap<object>();
        }

        private CodegenExpression CodegenEvaluateGetInternal(
            CodegenExpression @event,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var block = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(PropertyDotScalarArrayForge), codegenClassScope)
                .AddParam(typeof(EventBean), "event").Block
                .DeclareVar(
                    getterReturnType, "value",
                    CodegenLegoCast.CastSafeFromObjectType(
                        getterReturnType,
                        getter.EventBeanGetCodegen(Ref("event"), codegenMethodScope, codegenClassScope)))
                .IfRefNullReturnNull("value");
            CodegenMethod method;
            if (ComponentTypeCollection.IsPrimitive) {
                method = block.MethodReturn(NewInstance(typeof(ArrayWrappingCollection), Ref("value")));
            }
            else {
                method = block.MethodReturn(Ref("value"));
            }

            return LocalMethodBuild(method).Pass(@event).Call();
        }
    }
} // end of namespace