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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class PropertyDotEventCollectionForge : ExprEnumerationForge,
        ExprEnumerationEval,
        ExprEnumerationGivenEvent,
        ExprEnumerationGivenEventForge,
        ExprNodeRenderable
    {
        private readonly bool _disablePropertyExpressionEventCollCache;
        private readonly EventType _fragmentType;
        private readonly EventPropertyGetterSPI _getter;

        private readonly string _propertyNameCache;
        private readonly int _streamId;

        public PropertyDotEventCollectionForge(
            string propertyNameCache,
            int streamId,
            EventType fragmentType,
            EventPropertyGetterSPI getter,
            bool disablePropertyExpressionEventCollCache)
        {
            _propertyNameCache = propertyNameCache;
            _streamId = streamId;
            _fragmentType = fragmentType;
            _getter = getter;
            _disablePropertyExpressionEventCollCache = disablePropertyExpressionEventCollCache;
        }

        public ExprEnumerationEval ExprEvaluatorEnumeration => this;

        public Type ComponentTypeCollection => null;

        public ExprNodeRenderable EnumForgeRenderable => this;

        public ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var eventInQuestion = eventsPerStream[_streamId];
            if (eventInQuestion == null) {
                return null;
            }

            return EvaluateInternal(eventInQuestion, context);
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegenImpl(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(FlexCollection),
                typeof(PropertyDotEventCollectionForge),
                codegenClassScope);

            var refEPS = exprSymbol.GetAddEPS(methodNode);

            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamId)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(
                    CodegenEvaluateInternal(
                        Ref("@event"),
                        method => exprSymbol.GetAddExprEvalCtx(method),
                        methodNode,
                        codegenClassScope));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateGetROCollectionEventsCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return EvaluateGetROCollectionEventsCodegenImpl(
                codegenMethodScope,
                exprSymbol,
                codegenClassScope);
        }

        public CodegenExpression EvaluateEventGetROCollectionEventsCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = methodScope.MakeChild(
                typeof(FlexCollection),
                typeof(PropertyDotEventCollectionForge),
                codegenClassScope);
            methodNode.Block
                .IfNullReturnNull(symbols.GetAddEvent(methodNode))
                .MethodReturn(
                    CodegenEvaluateInternal(
                        symbols.GetAddEvent(methodNode),
                        method => symbols.GetAddExprEvalCtx(method),
                        methodNode,
                        codegenClassScope));
            return LocalMethod(methodNode);
        }

        public ICollection<EventBean> EvaluateEventGetROCollectionEvents(
            EventBean @event,
            ExprEvaluatorContext context)
        {
            if (@event == null) {
                return null;
            }

            return EvaluateInternal(@event, context);
        }

        public EventType GetEventTypeCollection(
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            return _fragmentType;
        }

        public ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public CodegenExpression EvaluateEventGetROCollectionScalarCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression EvaluateGetROCollectionScalarCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

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

        public CodegenExpression EvaluateEventGetEventBeanCodegen(
            CodegenMethodScope methodScope,
            ExprEnumerationGivenEventSymbol symbols,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
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

        public CodegenExpression EvaluateGetEventBeanCodegen(
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        private ICollection<EventBean> EvaluateInternal(
            EventBean eventInQuestion,
            ExprEvaluatorContext context)
        {
            var events = (EventBean[])_getter.GetFragment(eventInQuestion);
            return events?.Unwrap<EventBean>();
        }

        private CodegenExpression CodegenEvaluateInternal(
            CodegenExpressionRef @event,
            Func<CodegenMethod, CodegenExpressionRef> refExprEvalCtxFunc,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (_disablePropertyExpressionEventCollCache) {
                var methodNodeX = codegenMethodScope.MakeChild(
                        typeof(FlexCollection),
                        typeof(PropertyDotEventCollectionForge),
                        codegenClassScope)
                    .AddParam<EventBean>("@event");

                methodNodeX.Block
                    .DeclareVar<EventBean[]>(
                        "events",
                        Cast(
                            typeof(EventBean[]),
                            _getter.EventBeanFragmentCodegen(Ref("@event"), methodNodeX, codegenClassScope)))
                    .IfRefNullReturnNull("events")
                    .MethodReturn(FlexWrap(Ref("events")));
                return LocalMethod(methodNodeX, @event);
            }

            var methodNode = codegenMethodScope
                .MakeChild(typeof(FlexCollection), typeof(PropertyDotEventCollectionForge), codegenClassScope)
                .AddParam<EventBean>("@event");
            var refExprEvalCtx = refExprEvalCtxFunc.Invoke(methodNode);

            methodNode.Block
                .DeclareVar<ExpressionResultCacheForPropUnwrap>(
                    "cache",
                    ExprDotMethodChain(refExprEvalCtx)
                        .Get("ExpressionResultCacheService")
                        .Get("AllocateUnwrapProp"))
                .DeclareVar<ExpressionResultCacheEntryBeanAndCollBean>(
                    "cacheEntry",
                    ExprDotMethod(Ref("cache"), "GetPropertyColl", Constant(_propertyNameCache), Ref("@event")))
                .IfCondition(NotEqualsNull(Ref("cacheEntry")))
                .BlockReturn(ExprDotName(Ref("cacheEntry"), "Result"))
                .DeclareVar<EventBean[]>(
                    "events",
                    Cast(
                        typeof(EventBean[]),
                        _getter.EventBeanFragmentCodegen(Ref("@event"), methodNode, codegenClassScope)))
                .DeclareVarNoInit(typeof(FlexCollection), "coll")
                .IfRefNull("events")
                .AssignRef("coll", ConstantNull())
                .IfElse()
                .AssignRef("coll", FlexWrap(Ref("events")))
                .BlockEnd()
                .Expression(
                    ExprDotMethod(
                        Ref("cache"),
                        "SavePropertyColl",
                        Constant(_propertyNameCache),
                        Ref("@event"),
                        Ref("coll")))
                .MethodReturn(Ref("coll"));
            return LocalMethod(methodNode, @event);
        }

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(GetType().GetSimpleName());
        }
    }
} // end of namespace