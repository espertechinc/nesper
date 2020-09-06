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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.allofanyof
{
	public class EnumAllOfAnyOfEventPlus : ThreeFormEventPlus
	{
		private readonly bool all;

		public EnumAllOfAnyOfEventPlus(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType indexEventType,
			int numParameters,
			bool all)
			: base(lambda, indexEventType, numParameters)
		{
			this.all = all;
		}

		public override EnumEval EnumEvaluator {
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

						var beans = (ICollection<EventBean>) enumcoll;
						var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
						var props = indexEvent.Properties;
						props[1] = enumcoll.Count;
						eventsLambda[StreamNumLambda + 1] = indexEvent;
						var count = -1;

						foreach (var next in beans) {
							count++;
							props[0] = count;
							eventsLambda[StreamNumLambda] = next;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (all) {
								if (pass == null || false.Equals(pass)) {
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
