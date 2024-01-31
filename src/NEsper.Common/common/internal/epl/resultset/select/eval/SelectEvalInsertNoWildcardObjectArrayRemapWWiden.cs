///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardObjectArrayRemapWWiden : SelectExprProcessorForge
    {
        internal readonly SelectExprForgeContext context;
        internal readonly EventType resultEventType;
        internal readonly int[] remapped;
        internal readonly TypeWidenerSPI[] wideners;

        public SelectEvalInsertNoWildcardObjectArrayRemapWWiden(
            SelectExprForgeContext context,
            EventType resultEventType,
            int[] remapped,
            TypeWidenerSPI[] wideners)
        {
            this.context = context;
            this.resultEventType = resultEventType;
            this.remapped = remapped;
            this.wideners = wideners;
        }

        public EventType ResultEventType => resultEventType;

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventTypeExpr,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return ProcessCodegen(
                resultEventTypeExpr,
                eventBeanFactory,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope,
                context.ExprForges,
                resultEventType.PropertyNames,
                remapped,
                wideners);
        }

        public static CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprForge[] forges,
            string[] propertyNames,
            int[] remapped,
            TypeWidenerSPI[] optionalWideners)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                typeof(SelectEvalInsertNoWildcardObjectArrayRemapWWiden),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<object[]>("result", NewArrayByLength(typeof(object), Constant(propertyNames.Length)));
            for (var i = 0; i < forges.Length; i++) {
                CodegenExpression value;
                if (optionalWideners != null && optionalWideners[i] != null) {
                    value = forges[i]
                        .EvaluateCodegen(forges[i].EvaluationType, methodNode, exprSymbol, codegenClassScope);
                    value = optionalWideners[i].WidenCodegen(value, codegenMethodScope, codegenClassScope);
                }
                else {
                    value = forges[i].EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope);
                }

                block.AssignArrayElement(Ref("result"), Constant(remapped[i]), value);
            }

            block.MethodReturn(
                ExprDotMethod(eventBeanFactory, "AdapterForTypedObjectArray", Ref("result"), resultEventType));
            return methodNode;
        }
    }
} // end of namespace