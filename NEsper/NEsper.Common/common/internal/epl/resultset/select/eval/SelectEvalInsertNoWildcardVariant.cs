///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalInsertNoWildcardVariant : SelectEvalBaseMap
    {
        private readonly VariantEventType variantEventType;
        private readonly EventType innerEventType;

        public SelectEvalInsertNoWildcardVariant(
            SelectExprForgeContext selectExprForgeContext,
            EventType resultEventType,
            VariantEventType variantEventType,
            EventType innerEventType)
            : base(selectExprForgeContext, resultEventType)

        {
            this.variantEventType = variantEventType;
            this.innerEventType = innerEventType;
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
            CodegenExpressionField variantType = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
            CodegenExpressionField innerType = codegenClassScope.AddFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(innerEventType, EPStatementInitServicesConstants.REF));
            CodegenExpression inner = ExprDotMethod(eventBeanFactory, "AdapterForTypedMap", props, innerType);
            return ExprDotMethod(variantType, "GetValueAddEventBean", inner);
        }
    }
} // end of namespace