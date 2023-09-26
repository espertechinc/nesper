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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertWildcardVariantWrapper : SelectEvalBaseMap
    {
        private readonly VariantEventType variantEventType;
        private readonly EventType wrappingEventType;

        public SelectEvalInsertWildcardVariantWrapper(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            VariantEventType variantEventType,
            EventType wrappingEventType)
            : base(selectExprForgeContext, resultEventType)

        {
            this.variantEventType = variantEventType;
            this.wrappingEventType = wrappingEventType;
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
            var type = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
            var innerType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(wrappingEventType, EPStatementInitServicesConstants.REF));
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var wrapped = ExprDotMethod(
                eventBeanFactory,
                "AdapterForTypedWrapper",
                ArrayAtIndex(refEPS, Constant(0)),
                Ref("props"),
                innerType);
            return ExprDotMethod(type, "GetValueAddEventBean", wrapped);
        }
    }
} // end of namespace