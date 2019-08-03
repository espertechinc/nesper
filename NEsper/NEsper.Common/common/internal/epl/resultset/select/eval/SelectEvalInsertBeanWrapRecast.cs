///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
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
            this.eventType = targetType;
            this.streamNumber = streamNumber;

            EventType sourceType = typesPerStream[streamNumber];
            Type sourceClass = sourceType.UnderlyingType;
            Type targetClass = targetType.UnderlyingEventType.UnderlyingType;
            if (!TypeHelper.IsSubclassOrImplementsInterface(sourceClass, targetClass)) {
                throw SelectEvalInsertUtil.MakeEventTypeCastException(sourceType, targetType);
            }
        }

        public EventType ResultEventType {
            get => eventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField memberUndType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(
                    eventType.UnderlyingEventType,
                    EPStatementInitServicesConstants.REF));
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                this.GetType(),
                codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("theEvent", ArrayAtIndex(refEPS, Constant(streamNumber)))
                .DeclareVar<EventBean>(
                    "recast",
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedBean",
                        ExprDotUnderlying(@Ref("theEvent")),
                        memberUndType))
                .MethodReturn(
                    ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedWrapper",
                        @Ref("recast"),
                        StaticMethod(typeof(Collections), "GetEmptyMap"),
                        resultEventType));
            return methodNode;
        }
    }
} // end of namespace