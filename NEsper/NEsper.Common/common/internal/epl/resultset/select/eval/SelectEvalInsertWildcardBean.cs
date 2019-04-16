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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardBean : SelectEvalBase,
        SelectExprProcessorForge
    {
        public SelectEvalInsertWildcardBean(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
            : base(selectExprForgeContext, resultEventType)

        {
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);
            CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
            CodegenExpression bean = ExprDotMethod(ArrayAtIndex(refEPS, Constant(0)), "getUnderlying");
            methodNode.Block.MethodReturn(ExprDotMethod(eventBeanFactory, "adapterForTypedBean", bean, resultEventType));
            return methodNode;
        }
    }
} // end of namespace