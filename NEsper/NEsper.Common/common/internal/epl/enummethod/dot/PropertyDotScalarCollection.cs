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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotScalarCollection : ExprEnumerationEval,
        ExprEnumerationForge,
        ExprEnumerationGivenEvent,
        ExprEnumerationGivenEventForge,
        ExprNodeRenderable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventPropertyGetterSPI getter;

        private readonly string propertyName;
        private readonly int streamId;

        public PropertyDotScalarCollection(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter,
            Type componentType)
        {
            this.propertyName = propertyName;
            this.streamId = streamId;
            this.getter = getter;
            ComponentTypeCollection = componentType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(eventsPerStream[streamId]);
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
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>), typeof(PropertyDotScalarCollection), codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block.MethodReturn(
                CodegenEvaluateInternal(ArrayAtIndex(refEPS, Constant(streamId)), codegenClassScope, methodNode));
            return LocalMethod(methodNode);
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

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public ExprNodeRenderable ForgeRenderable => this;

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<EventBean>(@event);
        }

        public ICollection<object> EvaluateEventGetROCollectionScalar(
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
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<object>), typeof(PropertyDotScalarCollection), codegenClassScope);
            methodNode.Block.MethodReturn(
                CodegenLegoCast.CastSafeFromObjectType(
                    typeof(ICollection<object>),
                    getter.EventBeanGetCodegen(
                        symbols.GetAddEvent(methodNode), codegenMethodScope, codegenClassScope)));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateEventGetEventBeanCodegen(
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
            writer.Write(GetType().GetSimpleName());
        }

        private ICollection<T> EvaluateInternal<T>(EventBean @event)
        {
            var result = getter.Get(@event);
            if (result == null) {
                return null;
            }

            if (!result.GetType().IsGenericCollection()) {
                Log.Warn(
                    "Expected collection-type input from property '" + propertyName + "' but received " +
                    result.GetType());
                return null;
            }

            return getter.Get(@event).Unwrap<T>();
        }

        private CodegenExpression CodegenEvaluateInternal(
            CodegenExpression @event,
            CodegenClassScope codegenClassScope,
            CodegenMethodScope codegenMethodScope)
        {
            var method = codegenMethodScope
                .MakeChild(typeof(ICollection<object>), typeof(PropertyDotScalarCollection), codegenClassScope)
                .AddParam(typeof(EventBean), "event").Block
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        typeof(ICollection<object>),
                        getter.EventBeanGetCodegen(Ref("event"), codegenMethodScope, codegenClassScope)));
            return LocalMethodBuild(method).Pass(@event).Call();
        }
    }
} // end of namespace