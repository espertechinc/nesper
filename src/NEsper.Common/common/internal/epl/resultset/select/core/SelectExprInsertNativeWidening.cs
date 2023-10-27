///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprInsertNativeWidening : SelectExprInsertNativeBase
    {
        private readonly TypeWidenerSPI[] wideners;

        public SelectExprInsertNativeWidening(
            EventType eventType,
            EventBeanManufacturerForge eventManufacturer,
            ExprForge[] exprForges,
            TypeWidenerSPI[] wideners) : base(eventType, eventManufacturer, exprForges)
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
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            var manufacturer = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventBeanManufacturer),
                eventManufacturer.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
            methodNode.Block
                .DeclareVar<object[]>(
                    "values",
                    NewArrayByLength(typeof(object), Constant(exprForges.Length)));
            new CodegenRepetitiveLengthBuilder(exprForges.Length, methodNode, codegenClassScope, GetType())
                .AddParam<object[]>("values")
                .SetConsumer(
                    (
                        index,
                        leaf) => {
                        var evalType = exprForges[index].EvaluationType;
                        var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                            evalType,
                            exprForges[index],
                            leaf,
                            exprSymbol,
                            codegenClassScope);
                        if (wideners[index] == null) {
                            leaf.Block.AssignArrayElement("values", Constant(index), expression);
                        }
                        else {
                            var refname = "evalResult" + index;
                            if (evalType == null) {
                                // no action
                            }
                            else {
                                var evalClass = evalType;
                                leaf.Block.DeclareVar(evalClass, refname, expression);
                                if (!evalClass.IsPrimitive) {
                                    leaf.Block.IfRefNotNull(refname)
                                        .AssignArrayElement(
                                            "values",
                                            Constant(index),
                                            wideners[index].WidenCodegen(Ref(refname), leaf, codegenClassScope))
                                        .BlockEnd();
                                }
                                else {
                                    leaf.Block.AssignArrayElement(
                                        "values",
                                        Constant(index),
                                        wideners[index].WidenCodegen(Ref(refname), leaf, codegenClassScope));
                                }
                            }
                        }
                    })
                .Build();
            methodNode.Block.MethodReturn(ExprDotMethod(manufacturer, "Make", Ref("values")));
            return methodNode;
        }
    }
} // end of namespace