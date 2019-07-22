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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalNoWildcardMap : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext selectContext;
        private readonly EventType resultEventType;

        public SelectEvalNoWildcardMap(
            SelectExprForgeContext selectContext,
            EventType resultEventType)
        {
            this.selectContext = selectContext;
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                this.GetType(),
                codegenClassScope);
            methodNode.Block.DeclareVar<IDictionary<object, object>>(
                "props",
                NewInstance(
                    typeof(Dictionary<object, object>),
                    Constant(CollectionUtil.CapacityHashMap(selectContext.ColumnNames.Length))));
            for (int i = 0; i < selectContext.ColumnNames.Length; i++) {
                CodegenExpression expression = CodegenLegoMayVoid.ExpressionMayVoid(
                    typeof(object),
                    selectContext.ExprForges[i],
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                methodNode.Block.Expression(
                    ExprDotMethod(@Ref("props"), "put", Constant(selectContext.ColumnNames[i]), expression));
            }

            methodNode.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "adapterForTypedMap", @Ref("props"), resultEventType));
            return methodNode;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }
    }
} // end of namespace