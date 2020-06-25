///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.allofanyof
{
	public class EnumAllOfAnyOfEvent : ThreeFormEventPlain
	{
		private readonly bool all;

		public EnumAllOfAnyOfEvent(
			ExprDotEvalParamLambda lambda,
			bool all) : base(lambda)
		{
			this.all = all;
		}

		public EnumEval EnumEvaluator {
			get {
				ExprEvaluator inner = InnerExpression.ExprEvaluator;
				return new ProxyEnumEval() {
					ProcEvaluateEnumMethod = (
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
						if (enumcoll.IsEmpty()) {
							return all;
						}

						ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
						foreach (EventBean next in beans) {
							eventsLambda[StreamNumLambda] = next;

							object pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (all) {
								if (pass == null || (!(Boolean) pass)) {
									return false;
								}
							}
							else {
								if (pass != null && ((Boolean) pass)) {
									return true;
								}
							}
						}

						return all;
					},
				};
			}
		}

		public override Type ReturnType()
		{
			return typeof(bool);
		}

		public override CodegenExpression ReturnIfEmptyOptional()
		{
			return Constant(all);
		}

		public override void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			CodegenLegoBooleanExpression.CodegenReturnBoolIfNullOrBool(
				block,
				InnerExpression.EvaluationType,
				InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope),
				all,
				all ? false : (bool?) null,
				!all,
				!all);
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(Constant(all));
		}
	}
} // end of namespace
