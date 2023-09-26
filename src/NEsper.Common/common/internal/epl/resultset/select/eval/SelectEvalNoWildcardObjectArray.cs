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
using com.espertech.esper.common.@internal.bytecodemodel.util;
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
            var method = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            method.Block
                .DeclareVar<object[]>(
                    "props",
                    NewArrayByLength(typeof(object), Constant(context.ExprForges.Length)));
            new CodegenRepetitiveLengthBuilder(context.ExprForges.Length, method, codegenClassScope, GetType())
                .AddParam<object[]>("props")
                .SetConsumer(
                    (
                        index,
                        leaf) => {
                        var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                            typeof(object),
                            context.ExprForges[index],
                            leaf,
                            exprSymbol,
                            codegenClassScope);
                        leaf.Block.AssignArrayElement("props", Constant(index), expression);
                    })
                .Build();

            method.Block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedObjectArray", Ref("props"), resultEventType));
            return method;
        }

        public EventType ResultEventType => resultEventType;
    }
} // end of namespace