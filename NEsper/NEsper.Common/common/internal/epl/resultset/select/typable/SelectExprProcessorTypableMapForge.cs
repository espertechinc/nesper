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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.typable
{
	public class SelectExprProcessorTypableMapForge : SelectExprProcessorTypableForge {
	    internal readonly EventType mapType;
	    internal readonly ExprForge innerForge;

	    public SelectExprProcessorTypableMapForge(EventType mapType, ExprForge innerForge) {
	        this.mapType = mapType;
	        this.innerForge = innerForge;
	    }

	    public ExprEvaluator ExprEvaluator
	    {
	        get => new SelectExprProcessorTypableMapEval(this);
	    }

	    public ExprForgeConstantType ForgeConstantType
	    {
	        get => ExprForgeConstantType.NONCONST;
	    }

	    public CodegenExpression EvaluateCodegen(Type requiredType, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope) {
	        return SelectExprProcessorTypableMapEval.Codegen(this, codegenMethodScope, exprSymbol, codegenClassScope);
	    }

	    public Type UnderlyingEvaluationType
	    {
	        get => typeof(IDictionary<object, object>);
	    }

	    public Type EvaluationType
	    {
	        get => typeof(EventBean);
	    }

	    public ExprForge InnerForge
	    {
	        get => innerForge;
	    }

	    public ExprNodeRenderable ForgeRenderable
	    {
	        get => innerForge.ForgeRenderable;
	    }

	    public EventType MapType
	    {
	        get => mapType;
	    }
	}
} // end of namespace