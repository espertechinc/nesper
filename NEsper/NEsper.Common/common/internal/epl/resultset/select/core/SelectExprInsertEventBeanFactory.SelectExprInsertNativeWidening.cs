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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class SelectExprInsertNativeWidening : SelectExprInsertNativeBase
        {
            private readonly TypeWidenerSPI[] wideners;

            public SelectExprInsertNativeWidening(
                EventType eventType,
                EventBeanManufacturerForge eventManufacturer,
                ExprForge[] exprForges,
                TypeWidenerSPI[] wideners)
                : base(eventType, eventManufacturer, exprForges)
            {
                this.wideners = wideners;
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var manufacturer = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    eventManufacturer.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
                var block = methodNode.Block
                    .DeclareVar<object[]>(
                        "values",
                        CodegenExpressionBuilder.NewArrayByLength(
                            typeof(object),
                            CodegenExpressionBuilder.Constant(exprForges.Length)));
                for (var i = 0; i < exprForges.Length; i++) {
                    var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                        exprForges[i].EvaluationType,
                        exprForges[i],
                        methodNode,
                        exprSymbol,
                        codegenClassScope);
                    if (wideners[i] == null) {
                        block.AssignArrayElement("values", CodegenExpressionBuilder.Constant(i), expression);
                    }
                    else {
                        var refname = "evalResult" + i;
                        block.DeclareVar(exprForges[i].EvaluationType, refname, expression);
                        if (exprForges[i].EvaluationType.CanBeNull()) {
                            block.IfRefNotNull(refname)
                                .AssignArrayElement(
                                    "values",
                                    CodegenExpressionBuilder.Constant(i),
                                    wideners[i]
                                        .WidenCodegen(
                                            CodegenExpressionBuilder.Ref(refname),
                                            methodNode,
                                            codegenClassScope))
                                .BlockEnd();
                        }
                        else {
                            block.AssignArrayElement(
                                "values",
                                CodegenExpressionBuilder.Constant(i),
                                wideners[i]
                                    .WidenCodegen(
                                        CodegenExpressionBuilder.Ref(refname),
                                        methodNode,
                                        codegenClassScope));
                        }
                    }
                }

                block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        manufacturer,
                        "Make",
                        CodegenExpressionBuilder.Ref("values")));
                return methodNode;
            }
        }
    }
}