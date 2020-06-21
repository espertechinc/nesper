///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationMethodSortedSubmapForge : AggregationMethodForge
	{
		private readonly ExprNode _fromKey;
		private readonly ExprNode _fromInclusive;
		private readonly ExprNode _toKey;
		private readonly ExprNode _toInclusive;
		private readonly Type _underlyingClass;
		private readonly AggregationMethodSortedEnum _aggMethod;
		private readonly Type _resultType;

		public AggregationMethodSortedSubmapForge(
			ExprNode fromKey,
			ExprNode fromInclusive,
			ExprNode toKey,
			ExprNode toInclusive,
			Type underlyingClass,
			AggregationMethodSortedEnum aggMethod,
			Type resultType)
		{
			_fromKey = fromKey;
			_fromInclusive = fromInclusive;
			_toKey = toKey;
			_toInclusive = toInclusive;
			_underlyingClass = underlyingClass;
			_aggMethod = aggMethod;
			_resultType = resultType;
		}

		public CodegenExpression CodegenCreateReader(
			CodegenMethodScope parent,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenMethod method = parent.MakeChild(typeof(AggregationMultiFunctionAggregationMethod), GetType(), classScope);
			method.Block
				.DeclareVar<ExprEvaluator>("fromKeyEval", ExprNodeUtilityCodegen.CodegenEvaluator(_fromKey.Forge, method, GetType(), classScope))
				.DeclareVar(
					typeof(ExprEvaluator),
					"fromInclusiveEval",
					ExprNodeUtilityCodegen.CodegenEvaluator(_fromInclusive.Forge, method, GetType(), classScope))
				.DeclareVar<ExprEvaluator>("toKeyEval", ExprNodeUtilityCodegen.CodegenEvaluator(_toKey.Forge, method, GetType(), classScope))
				.DeclareVar(
					typeof(ExprEvaluator),
					"toInclusiveEval",
					ExprNodeUtilityCodegen.CodegenEvaluator(_toInclusive.Forge, method, GetType(), classScope))
				.MethodReturn(
					StaticMethod(
						typeof(AggregationMethodSortedSubmapFactory),
						"MakeSortedAggregationSubmap",
						Ref("fromKeyEval"),
						Ref("fromInclusiveEval"),
						Ref("toKeyEval"),
						Ref("toInclusiveEval"),
						EnumValue(typeof(AggregationMethodSortedEnum), _aggMethod.GetName()),
						Constant(_underlyingClass)));
			return LocalMethod(method);
		}

		public Type ResultType => _resultType;
	}
} // end of namespace
