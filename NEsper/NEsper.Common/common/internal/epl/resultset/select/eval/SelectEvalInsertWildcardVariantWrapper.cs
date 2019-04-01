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
	public class SelectEvalInsertWildcardVariantWrapper : SelectEvalBaseMap {

	    private readonly VariantEventType variantEventType;
	    private readonly EventType wrappingEventType;

	    public SelectEvalInsertWildcardVariantWrapper(SelectExprForgeContext selectExprForgeContext, EventType resultEventType, VariantEventType variantEventType, EventType wrappingEventType)

	    	 : base(selectExprForgeContext, resultEventType)

	    {
	        this.variantEventType = variantEventType;
	        this.wrappingEventType = wrappingEventType;
	    }

	    protected override CodegenExpression ProcessSpecificCodegen(CodegenExpression resultEventType, CodegenExpression eventBeanFactory, CodegenExpression props, CodegenMethod methodNode, SelectExprProcessorCodegenSymbol selectEnv, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField type = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
	        CodegenExpressionField innerType = codegenClassScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(wrappingEventType, EPStatementInitServicesConstants.REF));
	        CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
	        CodegenExpression wrapped = ExprDotMethod(eventBeanFactory, "adapterForTypedWrapper", ArrayAtIndex(refEPS, Constant(0)), @Ref("props"), innerType);
	        return ExprDotMethod(type, "getValueAddEventBean", wrapped);
	    }
	}
} // end of namespace