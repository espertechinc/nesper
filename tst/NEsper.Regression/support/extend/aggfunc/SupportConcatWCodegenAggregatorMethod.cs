///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
	public class SupportConcatWCodegenAggregatorMethod : AggregatorMethod
	{

		private CodegenExpressionMember builder;

		public SupportConcatWCodegenAggregatorMethod(AggregatorMethodFactoryContext context)
		{
		}

		public void InitForge(
			int col,
			CodegenCtor rowCtor,
			CodegenMemberCol membersColumnized,
			CodegenClassScope classScope)
		{
			builder = membersColumnized.AddMember(col, typeof(StringBuilder), "buf");
			rowCtor.Block.AssignRef(builder, NewInstance(typeof(StringBuilder)));
		}

		public void ApplyEvalEnterCodegen(
			CodegenMethod method,
			ExprForgeCodegenSymbol symbols,
			ExprForge[] forges,
			CodegenClassScope classScope)
		{
			method.Block
				.DeclareVar(
					typeof(string),
					"value",
					Cast(typeof(string), forges[0].EvaluateCodegen(typeof(string), method, symbols, classScope)))
				.ExprDotMethod(builder, "append", @Ref("value"));
		}

		public void ApplyEvalLeaveCodegen(
			CodegenMethod method,
			ExprForgeCodegenSymbol symbols,
			ExprForge[] forges,
			CodegenClassScope classScope)
		{

		}

		public void ApplyTableEnterCodegen(
			CodegenExpressionRef value,
			Type[] evaluationTypes,
			CodegenMethod method,
			CodegenClassScope classScope)
		{

		}

		public void ApplyTableLeaveCodegen(
			CodegenExpressionRef value,
			Type[] evaluationTypes,
			CodegenMethod method,
			CodegenClassScope classScope)
		{

		}

		public void ClearCodegen(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block.AssignRef(builder, NewInstance(typeof(StringBuilder)));
		}

		public void GetValueCodegen(
			CodegenMethod method,
			CodegenClassScope classScope)
		{
			method.Block.MethodReturn(ExprDotMethod(builder, "toString"));
		}

		public void WriteCodegen(
			CodegenExpressionRef row,
			int col,
			CodegenExpressionRef output,
			CodegenExpressionRef unitKey,
			CodegenExpressionRef writer,
			CodegenMethod method,
			CodegenClassScope classScope)
		{

		}

		public void ReadCodegen(
			CodegenExpressionRef row,
			int col,
			CodegenExpressionRef input,
			CodegenExpressionRef unitKey,
			CodegenMethod method,
			CodegenClassScope classScope)
		{

		}

		public void CollectFabricType(FabricTypeCollector collector)
		{

		}
	}
} // end of namespace
