///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertBeanWrapRecast : SelectExprProcessorForge
    {
        private readonly WrapperEventType eventType;
        private readonly int streamNumber;

        public SelectEvalInsertBeanWrapRecast(
            WrapperEventType targetType,
            int streamNumber,
            EventType[] typesPerStream)
        {
            eventType = targetType;
            this.streamNumber = streamNumber;

            var sourceType = typesPerStream[streamNumber];
            var sourceClass = sourceType.UnderlyingType;
            var targetClass = targetType.UnderlyingEventType.UnderlyingType;
            if (!TypeHelper.IsSubclassOrImplementsInterface(sourceClass, targetClass)) {
                throw SelectEvalInsertUtil.MakeEventTypeCastException(sourceType, targetType);
            }
        }

        public EventType ResultEventType => eventType;

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var memberUndType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    eventType.UnderlyingEventType,
                    EPStatementInitServicesConstants.REF));
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(refEPS, Constant(streamNumber)))
                .DeclareVar<EventBean>(
                    "recast",
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedObject",
                        ExprDotUnderlying(Ref("theEvent")),
                        memberUndType))
                .MethodReturn(
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedWrapper",
                        Ref("recast"),
                        StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) }),
                        resultEventType));
            return methodNode;
        }
    }
} // end of namespace