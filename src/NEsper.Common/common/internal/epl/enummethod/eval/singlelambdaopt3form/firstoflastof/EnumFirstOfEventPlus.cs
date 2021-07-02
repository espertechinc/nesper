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
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // ConstantNull

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
	public class EnumFirstOfEventPlus : ThreeFormEventPlus
	{
		public EnumFirstOfEventPlus(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType indexEventType,
			int numParameters) : base(lambda, indexEventType, numParameters)
		{
		}

		public override EnumEval EnumEvaluator {
			get {
				var inner = InnerExpression.ExprEvaluator;
				return new ProxyEnumEval() {
					ProcEvaluateEnumMethod = (
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
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
							if (pass == null || false.Equals(pass)) {
								continue;
							}

							return next;
						}

						return null;
					},
				};
			}
		}

		public override Type ReturnTypeOfMethod()
		{
			return typeof(EventBean);
		}

		public override CodegenExpression ReturnIfEmptyOptional()
		{
			return ConstantNull();
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
			CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
				block,
				InnerExpression.EvaluationType,
				InnerExpression.EvaluateCodegen(typeof(bool), methodNode, scope, codegenClassScope));
			block.BlockReturn(Ref("next"));
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(ConstantNull());
		}
	}
} // end of namespace
