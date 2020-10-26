///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
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
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                this.GetType(),
                codegenClassScope);
            var codegenPropsRef = Ref("props");

            methodNode.Block.DeclareVar<IDictionary<string, object>>(
                "props",
                NewInstance(typeof(HashMap<string, object>)));
            for (int i = 0; i < selectContext.ColumnNames.Length; i++) {
                var selectContextExprForge = selectContext.ExprForges[i];
                var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                    typeof(object),
                    selectContextExprForge,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);

                var codegenValue = Constant(selectContext.ColumnNames[i]);
                methodNode.Block.Expression(
                    ExprDotMethod(codegenPropsRef, "Put", codegenValue, expression));
            }

            methodNode.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedMap", Ref("props"), resultEventType));
            return methodNode;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }
    }
} // end of namespace