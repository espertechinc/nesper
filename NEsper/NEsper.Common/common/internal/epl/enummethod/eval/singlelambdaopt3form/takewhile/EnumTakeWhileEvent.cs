///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile
{
	public class EnumTakeWhileEvent : ThreeFormEventPlain
	{

		private CodegenExpression innerValue;

		public EnumTakeWhileEvent(ExprDotEvalParamLambda lambda) : base(lambda)
		{
		}

		public EnumEval EnumEvaluator {
			get {
				var inner = InnerExpression.ExprEvaluator;
				return new ProxyEnumEval() {
					ProcEvaluateEnumMethod = (
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
						if (enumcoll.IsEmpty()) {
							return enumcoll;
						}

						var beans = (ICollection<EventBean>) enumcoll;
						if (enumcoll.Count == 1) {
							var item = beans.First();
							eventsLambda[StreamNumLambda] = item;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || (!(Boolean) pass)) {
								return EmptyList<object>.Instance;
							}

							return Collections.SingletonList(item);
						}

						var result = new ArrayDeque<object>();

						foreach (var next in beans) {
							eventsLambda[StreamNumLambda] = next;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || (!(Boolean) pass)) {
								break;
							}

							result.Add(next);
						}

						return result;
					},
				};
			}
		}

		public override Type ReturnType()
		{
			return typeof(ICollection<object>);
		}

		public override CodegenExpression ReturnIfEmptyOptional()
		{
			return EnumForgeCodegenNames.REF_ENUMCOLL;
		}

		public override void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			innerValue = InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);
			EnumTakeWhileHelper.InitBlockSizeOneEvent(block, innerValue, StreamNumLambda, InnerExpression.EvaluationType);
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(block, InnerExpression.EvaluationType, innerValue);
			block.Expression(ExprDotMethod(Ref("result"), "Add", Ref("next")));
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(Ref("result"));
		}
	}
} // end of namespace
