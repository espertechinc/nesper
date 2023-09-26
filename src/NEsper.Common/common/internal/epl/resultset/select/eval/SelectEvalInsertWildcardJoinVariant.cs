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
using com.espertech.esper.common.@internal.@event.variant;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardJoinVariant : SelectEvalBase,
        SelectExprProcessorForge
    {
        private readonly SelectExprProcessorForge joinWildcardProcessorForge;
        private readonly VariantEventType variantEventType;

        public SelectEvalInsertWildcardJoinVariant(
            SelectExprForgeContext context,
            EventType resultEventType,
            SelectExprProcessorForge joinWildcardProcessorForge,
            VariantEventType variantEventType)
            : base(context, resultEventType)

        {
            this.joinWildcardProcessorForge = joinWildcardProcessorForge;
            this.variantEventType = variantEventType;
        }

        public CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var variantType = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
            var methodNode = codegenMethodScope.MakeChild(
                typeof(EventBean),
                GetType(),
                codegenClassScope);
            var jw = joinWildcardProcessorForge.ProcessCodegen(
                resultEventType,
                eventBeanFactory,
                methodNode,
                selectSymbol,
                exprSymbol,
                codegenClassScope);
            methodNode.Block.MethodReturn(ExprDotMethod(variantType, "GetValueAddEventBean", LocalMethod(jw)));
            return methodNode;
        }
    }
} // end of namespace