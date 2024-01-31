///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalBaseMap : SelectEvalBase,
        SelectExprProcessorForge
    {
        protected ExprEvaluator[] evaluators;

        protected SelectEvalBaseMap(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
            : base(selectExprForgeContext, resultEventType)

        {
        }

        protected abstract CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethod methodNode,
            SelectExprProcessorCodegenSymbol selectEnv,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

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
            var block = methodNode.Block;
            if (context.ExprForges.Length == 0) {
                block.DeclareVar<IDictionary<string, object>>(
                    "props",
                    StaticMethod(typeof(Collections), "GetEmptyMap", new[] { typeof(string), typeof(object) }));
            }
            else {
                block.DeclareVar<IDictionary<string, object>>(
                    "props",
                    NewInstance(typeof(HashMap<string, object>)));
            }

            for (var i = 0; i < context.ColumnNames.Length; i++) {
                var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                    typeof(object),
                    context.ExprForges[i],
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.Expression(
                    ExprDotMethod(Ref("props"), "Put", Constant(context.ColumnNames[i]), expression));
            }

            block.MethodReturn(
                ProcessSpecificCodegen(
                    resultEventType,
                    eventBeanFactory,
                    Ref("props"),
                    methodNode,
                    selectSymbol,
                    exprSymbol,
                    codegenClassScope));
            return methodNode;
        }
    }
} // end of namespace