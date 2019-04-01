///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	public class SelectEvalInsertNoWildcardSingleColCoercionBeanWrapVariant : SelectEvalBaseFirstProp {

	    private readonly VariantEventType variantEventType;

	    public SelectEvalInsertNoWildcardSingleColCoercionBeanWrapVariant(SelectExprForgeContext selectExprForgeContext, EventType resultEventType, VariantEventType variantEventType)

	    	 : base(selectExprForgeContext, resultEventType)

	    {
	        this.variantEventType = variantEventType;
	    }

	    protected override CodegenExpression ProcessFirstColCodegen(Type evaluationType, CodegenExpression expression, CodegenExpression resultEventType, CodegenExpression eventBeanFactory, CodegenMethodScope codegenMethodScope, CodegenClassScope codegenClassScope) {
	        CodegenExpressionField type = VariantEventTypeUtil.GetField(variantEventType, codegenClassScope);
	        CodegenMethod method = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope).AddParam(evaluationType, "result").Block
	                .DeclareVar(typeof(EventType), "beanEventType", ExprDotMethod(type, "eventTypeForNativeObject", @Ref("result")))
	                .DeclareVar(typeof(EventBean), "wrappedEvent", ExprDotMethod(eventBeanFactory, "adapterForTypedBean", @Ref("result"), @Ref("beanEventType")))
	                .DeclareVar(typeof(EventBean), "variant", ExprDotMethod(type, "getValueAddEventBean", @Ref("wrappedEvent")))
	                .MethodReturn(ExprDotMethod(eventBeanFactory, "adapterForTypedWrapper", @Ref("variant"), StaticMethod(typeof(Collections), "emptyMap"), resultEventType));
	        return LocalMethodBuild(method).Pass(expression).Call();
	    }
	}
} // end of namespace