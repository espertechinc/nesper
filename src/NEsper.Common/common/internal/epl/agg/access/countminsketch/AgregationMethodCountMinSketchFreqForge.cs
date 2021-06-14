///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.countminsketch
{
	public class AgregationMethodCountMinSketchFreqForge : AggregationMethodForge
	{
		private readonly ExprNode _frequencyEval;

		public AgregationMethodCountMinSketchFreqForge(ExprNode frequencyEval)
		{
			_frequencyEval = frequencyEval;
		}

		public Type ResultType => typeof(long?);

		public CodegenExpression CodegenCreateReader(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenMethod method = parent.MakeChild(typeof(AgregationMethodCountMinSketchFreq), this.GetType(), classScope);
			method.Block
				.DeclareVar<AgregationMethodCountMinSketchFreq>("strat", NewInstance(typeof(AgregationMethodCountMinSketchFreq)))
				.SetProperty(Ref("strat"), "FrequencyEval", ExprNodeUtilityCodegen.CodegenEvaluator(_frequencyEval.Forge, method, this.GetType(), classScope))
				.MethodReturn(Ref("strat"));
			return LocalMethod(method);
		}
	}
} // end of namespace
