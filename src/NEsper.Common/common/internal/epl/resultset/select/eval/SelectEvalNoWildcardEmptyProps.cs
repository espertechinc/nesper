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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalNoWildcardEmptyProps : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext selectExprForgeContext;
        private readonly EventType resultEventType;

        public SelectEvalNoWildcardEmptyProps(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
        {
            this.selectExprForgeContext = selectExprForgeContext;
            this.resultEventType = resultEventType;
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
            methodNode.Block.MethodReturn(
                ExprDotMethod(
                    eventBeanFactory,
                    "AdapterForTypedMap",
                    StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(object), typeof(object) }),
                    resultEventType));
            return methodNode;
        }

        public EventType ResultEventType => resultEventType;
    }
} // end of namespace