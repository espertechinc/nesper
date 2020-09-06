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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // Ref;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile
{
	public class EnumTakeWhileEventPlus : ThreeFormEventPlus
	{

		private CodegenExpression innerValue;

		public EnumTakeWhileEventPlus(
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
						if (enumcoll.IsEmpty()) {
							return enumcoll;
						}

						var beans = (ICollection<EventBean>) enumcoll;
						var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
						var props = indexEvent.Properties;
						props[0] = 0;
						props[1] = enumcoll.Count;
						eventsLambda[StreamNumLambda + 1] = indexEvent;

						if (enumcoll.Count == 1) {
							var item = beans.First();
							eventsLambda[StreamNumLambda] = item;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || false.Equals(pass)) {
								return FlexCollection.Empty;
							}

							return FlexCollection.OfEvent(item);
						}

						var result = new ArrayDeque<EventBean>();
						var count = -1;

						foreach (var next in beans) {
							count++;
							props[0] = count;
							eventsLambda[StreamNumLambda] = next;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || false.Equals(pass)) {
								break;
							}

							result.Add(next);
						}

						return FlexCollection.Of(result);
					},
				};
			}
		}

		public override Type ReturnType()
		{
			return typeof(FlexCollection);
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
			EnumTakeWhileHelper.InitBlockSizeOneEventPlus(numParameters, block, innerValue, StreamNumLambda, InnerExpression.EvaluationType);
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
			block.MethodReturn(FlexWrap(Ref("result")));
		}
	}
} // end of namespace
