///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotScalarIterable : ExprEnumerationForge,
        ExprEnumerationEval,
        ExprEnumerationGivenEvent,
        ExprEnumerationGivenEventForge,
        ExprNodeRenderable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string propertyName;
        private readonly int streamId;
        private readonly EventPropertyGetterSPI getter;
        private readonly Type componentType;
        private readonly Type getterReturnType;

        public PropertyDotScalarIterable(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter,
            Type componentType,
            Type getterReturnType)
        {
            this.propertyName = propertyName;
            this.streamId = streamId;
            this.getter = getter;
            this.componentType = componentType;
            this.getterReturnType = getterReturnType;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(eventsPerStream[streamId]);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var refEPS = exprSymbol.GetAddEps(codegenMethodScope);
            return CodegenEvaluateInternal(
                ArrayAtIndex(refEPS, Constant(streamId)),
                codegenMethodScope,
                codegenClassScope);
        }

        public CodegenExpression EvaluateEventGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            if (getterReturnType.IsGenericCollection()) {
                return getter.EventBeanGetCodegen(
                    symbols.GetAddEvent(codegenMethodScope),
                    codegenMethodScope,
                    codegenClassScope);
            }

            var method = codegenMethodScope.MakeChild(
                typeof(ICollection<object>),
                typeof(PropertyDotScalarIterable),
                codegenClassScope);
            method.Block.DeclareVar(
                    getterReturnType,
                    "result",
                    CodegenLegoCast.CastSafeFromObjectType(
                        typeof(IEnumerable),
                        getter.EventBeanGetCodegen(symbols.GetAddEvent(method), codegenMethodScope, codegenClassScope)))
                .IfRefNullReturnNull("result")
                .MethodReturn(StaticMethod(typeof(CollectionUtil), "IterableToCollection", Ref("result")));
            return LocalMethod(method);
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(@event);
        }

        private ICollection<T> EvaluateInternal<T>(EventBean @event)
        {
            var result = getter.Get(@event);
            if (result == null) {
                return null;
            }

            if (result.GetType().IsGenericCollection()) {
                return result.Unwrap<T>();
            }

            if (!(result is IEnumerable)) {
                Log.Warn(
                    "Expected enumerable-type input from property '" +
                    propertyName +
                    "' but received " +
                    result.GetType());
                return null;
            }

            return result.Unwrap<T>();
        }

        private CodegenExpression CodegenEvaluateInternal(
            CodegenExpression @event,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (getterReturnType.IsImplementsInterface(typeof(ICollection))) {
                return getter.EventBeanGetCodegen(@event, codegenMethodScope, codegenClassScope);
            }

            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(PropertyDotScalarIterable), codegenClassScope)
                .AddParam<EventBean>("@event")
                .Block
                .DeclareVar(
                    getterReturnType,
                    "result",
                    CodegenLegoCast.CastSafeFromObjectType(
                        getterReturnType,
                        getter.EventBeanGetCodegen(Ref("@event"), codegenMethodScope, codegenClassScope)))
                .IfRefNullReturnNull("result")
                .MethodReturn(StaticMethod(typeof(CompatExtensions), "UnwrapIntoList", new [] { typeof(object) }, Ref("result")));
            return LocalMethodBuild(method).Pass(@event).Call();
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public Type ComponentTypeCollection => componentType;

        public EventType GetEventTypeSingle(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return null;
        }

        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
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

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public EventBean EvaluateEventGetEventBean(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateEventGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprNodeRenderable ForgeRenderable => this;
        
        public ExprNodeRenderable EnumForgeRenderable => ForgeRenderable;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().Name);
        }
    }
} // end of namespace