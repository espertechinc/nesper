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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
	/// <summary>
	/// Processor for select-clause expressions that handles wildcards. Computes results based on matching events.
	/// </summary>
	public class SelectEvalJoinWildcardProcessorObjectArray : SelectExprProcessorForge {
	    private readonly string[] streamNames;
	    private readonly EventType resultEventType;

	    public SelectEvalJoinWildcardProcessorObjectArray(string[] streamNames, EventType resultEventType) {
	        this.streamNames = streamNames;
	        this.resultEventType = resultEventType;
	    }

	    public EventType ResultEventType {
	        get => resultEventType;
	    }

	    public CodegenMethod ProcessCodegen(CodegenExpression resultEventTypeOuter, CodegenExpression eventBeanFactory, CodegenMethodScope codegenMethodScope, SelectExprProcessorCodegenSymbol selectSymbol, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        // NOTE: Maintaining result-event-type as out own field as we may be an "inner" select-expr-processor
	        CodegenExpressionField mType = codegenClassScope.AddFieldUnshared(true, typeof(EventType), EventTypeUtility.ResolveTypeCodegen(resultEventType, EPStatementInitServicesConstants.REF));
	        CodegenMethod methodNode = codegenMethodScope.MakeChild(typeof(EventBean), this.GetType(), codegenClassScope);
	        CodegenExpressionRef refEPS = exprSymbol.GetAddEPS(methodNode);
	        methodNode.Block
	                .DeclareVar(typeof(object[]), "tuple", NewArrayByLength(typeof(object), Constant(streamNames.Length)))
	                .StaticMethod(typeof(System), "arraycopy", refEPS, Constant(0), @Ref("tuple"), Constant(0), Constant(streamNames.Length))
	                .MethodReturn(ExprDotMethod(eventBeanFactory, "adapterForTypedObjectArray", @Ref("tuple"), mType));
	        return methodNode;
	    }
	}
} // end of namespace