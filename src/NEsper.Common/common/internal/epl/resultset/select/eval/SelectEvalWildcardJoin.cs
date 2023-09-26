///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalWildcardJoin : SelectEvalBaseMap,
        SelectExprProcessorForge
    {
        private readonly SelectExprProcessorForge joinWildcardProcessorForge;

        public SelectEvalWildcardJoin(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            SelectExprProcessorForge joinWildcardProcessorForge)
            : base(selectExprForgeContext, resultEventType)

        {
            this.joinWildcardProcessorForge = joinWildcardProcessorForge;
        }

        protected override CodegenExpression ProcessSpecificCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenExpression props,
            CodegenMethod methodNode,
            SelectExprProcessorCodegenSymbol selectEnv,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var inner = joinWildcardProcessorForge.ProcessCodegen(
                resultEventType,
                eventBeanFactory,
                methodNode,
                selectEnv,
                exprSymbol,
                codegenClassScope);
            return ExprDotMethod(
                eventBeanFactory,
                "AdapterForTypedWrapper",
                LocalMethod(inner),
                props,
                resultEventType);
        }
    }
} // end of namespace