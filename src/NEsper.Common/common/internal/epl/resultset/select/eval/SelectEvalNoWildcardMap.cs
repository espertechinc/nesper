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
using com.espertech.esper.common.@internal.bytecodemodel.util;
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
                GetType(),
                codegenClassScope);
            var codegenPropsRef = Ref("props");

            methodNode.Block.DeclareVar<IDictionary<string, object>>(
                "props",
                NewInstance(typeof(HashMap<string, object>)));

            new CodegenRepetitiveLengthBuilder(
                    selectContext.ColumnNames.Length,
                    methodNode,
                    codegenClassScope,
                    GetType())
                .AddParam(typeof(IDictionary<string, object>), "props")
                .SetConsumer(
                    (
                        index,
                        leafMethod) => {
                        var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            selectContext.ExprForges[index],
                            leafMethod,
                            exprSymbol,
                            codegenClassScope);
                        leafMethod.Block.Expression(
                            ExprDotMethod(Ref("props"), "Put", Constant(selectContext.ColumnNames[index]), expression));
                    })
                .Build();

            methodNode.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedMap", Ref("props"), resultEventType));
            return methodNode;
        }

        public EventType ResultEventType => resultEventType;
    }
} // end of namespace