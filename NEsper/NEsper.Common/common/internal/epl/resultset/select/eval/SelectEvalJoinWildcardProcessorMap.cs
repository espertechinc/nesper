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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	/// <summary>
	/// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
	/// </summary>
	public class SelectEvalJoinWildcardProcessorMap : SelectExprProcessorForge {
	    private readonly string[] streamNames;
	    private readonly EventType resultEventType;

	    public SelectEvalJoinWildcardProcessorMap(string[] streamNames, EventType resultEventType) {
	        this.streamNames = streamNames;
	        this.resultEventType = resultEventType;
	    }

	    public CodegenMethod ProcessCodegen(CodegenExpression resultEventTypeOuter, CodegenExpression eventBeanFactory, CodegenMethodScope codegenMethodScope, SelectExprProcessorCodegenSymbol selectSymbol, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        // NOTE: Maintaining result-event-type as out own field as we may be an "inner" select-expr-processor
	        CodegenExpressionField mType = codegenClassScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(resultEventType, EPStatementInitServicesConstants.REF));
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);
	        CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
	        methodNode.Block
	                .DeclareVar(typeof(IDictionary<object, object>), "tuple", NewInstance(typeof(Dictionary<object, object>), Constant(CollectionUtil.CapacityHashMap(streamNames.Length))));
	        for (int i = 0; i < streamNames.Length; i++) {
	            methodNode.Block.Expression(ExprDotMethod(@Ref("tuple"), "put", Constant(streamNames[i]), ArrayAtIndex(refEPS, Constant(i))));
	        }
	        methodNode.Block.MethodReturn(ExprDotMethod(eventBeanFactory, "adapterForTypedMap", @Ref("tuple"), mType));
	        return methodNode;
	    }

	    public EventType ResultEventType {
	        get => resultEventType;
	    }
	}
} // end of namespace