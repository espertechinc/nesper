///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
	public class EnumSumScalarNoParams : EnumForgeBasePlain,
		EnumForge,
		EnumEval
	{
		private readonly ExprDotEvalSumMethodFactory _sumMethodFactory;

		public EnumSumScalarNoParams(
			int streamCountIncoming,
			ExprDotEvalSumMethodFactory sumMethodFactory) : base(streamCountIncoming)
		{
			this._sumMethodFactory = sumMethodFactory;
		}

		public override EnumEval EnumEvaluator => this;

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			var method = _sumMethodFactory.SumAggregator;
			foreach (var next in enumcoll) {
				method.Enter(next);
			}

			return method.Value;
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var scope = new ExprForgeCodegenSymbol(false, null);
			var methodNode = codegenMethodScope
				.MakeChildWithScope(_sumMethodFactory.ValueType, typeof(EnumSumScalarNoParams), scope, codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);
			var block = methodNode.Block;

			_sumMethodFactory.CodegenDeclare(block);

			var forEach = block
				.ForEach<object>("next", EnumForgeCodegenNames.REF_ENUMCOLL)
				.IfRefNull("next")
				.BlockContinue();
			_sumMethodFactory.CodegenEnterObjectTypedNonNull(forEach, Ref("next"));

			_sumMethodFactory.CodegenReturn(block);
			return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
		}
	}
} // end of namespace
