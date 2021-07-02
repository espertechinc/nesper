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
    public class SelectEvalInsertNoWildcardObjectArray : SelectEvalBase,
        SelectExprProcessorForge
    {
        public SelectEvalInsertNoWildcardObjectArray(
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
            CodegenMethod methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            CodegenBlock block = methodNode.Block
                .DeclareVar<object[]>(
                    "result",
                    NewArrayByLength(typeof(object), Constant(context.ExprForges.Length)));
            for (int i = 0; i < context.ExprForges.Length; i++) {
                CodegenExpression expression = CodegenLegoMayVoid.ExpressionMayVoid(
                    typeof(object),
                    context.ExprForges[i],
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.AssignArrayElement("result", Constant(i), expression);
            }

            block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedObjectArray", Ref("result"), resultEventType));
            return methodNode;
        }
    }
} // end of namespace