///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.access.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
	public class AggregationAgentCountMinSketchForge : AggregationAgentForge {

	    private readonly ExprForge stringEvaluator;
	    private readonly ExprForge optionalFilterForge;

	    public AggregationAgentCountMinSketchForge(ExprForge stringEvaluator, ExprForge optionalFilterForge) {
	        this.stringEvaluator = stringEvaluator;
	        this.optionalFilterForge = optionalFilterForge;
	    }

	    public ExprForge OptionalFilter {
	        get => optionalFilterForge;
	    }

	    public CodegenExpression Make(CodegenMethod parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        CodegenMethod method = parent.MakeChild(typeof(AggregationAgentCountMinSketch), this.GetType(), classScope);
	        method.Block
	                .DeclareVar(typeof(AggregationAgentCountMinSketch), "cms", NewInstance(typeof(AggregationAgentCountMinSketch)))
	                .ExprDotMethod(@Ref("cms"), "setStringEval", ExprNodeUtilityCodegen.CodegenEvaluator(stringEvaluator, method, this.GetType(), classScope))
	                .ExprDotMethod(@Ref("cms"), "setOptionalFilterEval", optionalFilterForge == null ? ConstantNull() : ExprNodeUtilityCodegen.CodegenEvaluator(optionalFilterForge, method, this.GetType(), classScope))
	                .MethodReturn(@Ref("cms"));
	        return LocalMethod(method);
	    }
	}
} // end of namespace