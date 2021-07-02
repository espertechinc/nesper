///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public abstract class SelectEvalBaseFirstProp : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext selectExprForgeContext;
        private readonly EventType resultEventType;

        public SelectEvalBaseFirstProp(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType)
        {
            this.selectExprForgeContext = selectExprForgeContext;
            this.resultEventType = resultEventType;
        }

        protected abstract CodegenExpression ProcessFirstColCodegen(
            Type evaluationType,
            CodegenExpression expression,
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope);

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var first = selectExprForgeContext.ExprForges[0];
            var evaluationType = first.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            if (evaluationType.IsNullType()) {
                methodNode.Block.MethodReturn(CodegenExpressionBuilder.ConstantNull());
            }
            else {
                methodNode.Block.MethodReturn(
                    ProcessFirstColCodegen(
                        evaluationType,
                        first.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope),
                        resultEventType,
                        eventBeanFactory,
                        methodNode,
                        codegenClassScope));
            }

            return methodNode;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }
    }
} // end of namespace