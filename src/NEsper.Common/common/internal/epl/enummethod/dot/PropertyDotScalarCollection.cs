///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

        private readonly string _propertyName;
        private readonly int _streamId;
        private readonly EventPropertyGetterSPI _getter;
        private readonly Type _componentType;

        public PropertyDotScalarCollection(
            string propertyName,
            int streamId,
            EventPropertyGetterSPI getter,
            Type componentType)
        {
            _propertyName = propertyName;
            _streamId = streamId;
            _getter = getter;
            _componentType = componentType;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<object>(eventsPerStream[_streamId]);
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(ICollection<>).MakeGenericType(_componentType),
                typeof(PropertyDotScalarCollection),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block.MethodReturn(
                CodegenEvaluateInternal(ArrayAtIndex(refEPS, Constant(_streamId)), codegenClassScope, methodNode));
            return LocalMethod(methodNode);
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal<EventBean>(@event);
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

        
        public EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
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
                typeof(ICollection<object>),
                typeof(PropertyDotScalarCollection),
                codegenClassScope);
            methodNode.Block.MethodReturn(
                CodegenLegoCast.CastSafeFromObjectType(
                    typeof(ICollection<object>),
                    _getter.EventBeanGetCodegen(
                        symbols.GetAddEvent(methodNode),
                        codegenMethodScope,
                        codegenClassScope)));
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

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
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

        public ExprNodeRenderable ForgeRenderable => this;

        public ExprNodeRenderable EnumForgeRenderable => ForgeRenderable;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().GetSimpleName());
        }

        private ICollection<T> EvaluateInternal<T>(EventBean @event)
        {
            var result = _getter.Get(@event);
            if (result == null) {
                return null;
            }

            if (!result.GetType().IsGenericCollection()) {
                Log.Warn(
                    "Expected collection-type input from property '" +
                    _propertyName +
                    "' but received " +
                    result.GetType());
                return null;
            }

            return _getter.Get(@event).Unwrap<T>();
        }

        private CodegenExpression CodegenEvaluateInternal(
            CodegenExpression @event,
            CodegenClassScope codegenClassScope,
            CodegenMethodScope codegenMethodScope)
        {
            var returnType = typeof(ICollection<>).MakeGenericType(_componentType);
            var method = codegenMethodScope
                .MakeChild(returnType, typeof(PropertyDotScalarCollection), codegenClassScope)
                .AddParam<EventBean>("@event")
                .Block
                .MethodReturn(
                    CodegenLegoCast.CastSafeFromObjectType(
                        returnType,
                        _getter.EventBeanGetCodegen(Ref("@event"), codegenMethodScope, codegenClassScope)));
            return LocalMethodBuild(method).Pass(@event).Call();
        }
    }
} // end of namespace