///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertBeanRecast : SelectExprProcessorForge
    {
        private readonly EventType eventType;
        private readonly int streamNumber;

        public SelectEvalInsertBeanRecast(
            EventType targetType,
            int streamNumber,
            EventType[] typesPerStream)
        {
            eventType = targetType;
            this.streamNumber = streamNumber;

            var sourceType = typesPerStream[streamNumber];
            var sourceClass = sourceType.UnderlyingType;
            var targetClass = targetType.UnderlyingType;
            if (!TypeHelper.IsSubclassOrImplementsInterface(sourceClass, targetClass)) {
                throw SelectEvalInsertUtil.MakeEventTypeCastException(sourceType, targetType);
            }
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);
            CodegenExpression bean = ExprDotName(ArrayAtIndex(refEPS, Constant(streamNumber)), "Underlying");
            methodNode.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedObject", bean, resultEventType));
            return methodNode;
        }

        public EventType ResultEventType => eventType;
    }
} // end of namespace