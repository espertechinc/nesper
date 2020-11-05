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
    public class SelectEvalNoWildcardObjectArray : SelectExprProcessorForge
    {
        private readonly SelectExprForgeContext context;
        private readonly EventType resultEventType;

        public SelectEvalNoWildcardObjectArray(
            SelectExprForgeContext context,
            EventType resultEventType)
        {
            this.context = context;
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
            CodegenBlock block = methodNode.Block
                .DeclareVar<object[]>(
                    "props",
                    NewArrayByLength(typeof(object), Constant(this.context.ExprForges.Length)));
            for (int i = 0; i < this.context.ExprForges.Length; i++) {
                CodegenExpression expression = CodegenLegoMayVoid.ExpressionMayVoid(
                    typeof(object),
                    this.context.ExprForges[i],
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                block.AssignArrayElement("props", Constant(i), expression);
            }

            block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedObjectArray", Ref("props"), resultEventType));
            return methodNode;
        }

        public EventType ResultEventType {
            get => resultEventType;
        }
    }
} // end of namespace