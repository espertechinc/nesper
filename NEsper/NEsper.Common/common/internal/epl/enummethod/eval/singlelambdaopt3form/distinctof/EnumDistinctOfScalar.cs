///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational; // LE

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof
{
	public class EnumDistinctOfScalar : ThreeFormScalar
	{
		private readonly Type _innerType;

		public EnumDistinctOfScalar(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType fieldEventType,
			int numParameters) : base(lambda, fieldEventType, numParameters)
		{
			_innerType = InnerExpression.EvaluationType.GetBoxedType();
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
						if (enumcoll.Count <= 1) {
							return enumcoll;
						}

						IDictionary<object, object> distinct = new Dictionary<object, object>();
						ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
						eventsLambda[StreamNumLambda] = resultEvent;
						object[] props = resultEvent.Properties;
						props[2] = enumcoll.Count;

						ICollection<object> values = (ICollection<object>) enumcoll;
						int count = -1;

						foreach (object next in values) {
							count++;
							props[1] = count;
							props[0] = next;

							object comparable = inner.Evaluate(eventsLambda, isNewData, context);
							if (!distinct.ContainsKey(comparable)) {
								distinct.Put(comparable, next);
							}
						}

						return distinct.Values;
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
			return null;
		}

		public override void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			methodNode.Block
				.IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
				.BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
				.DeclareVar(typeof(IDictionary<object, object>), "distinct", NewInstance(typeof(LinkedHashMap<object, object>)));
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			CodegenExpression eval = InnerExpression.EvaluateCodegen(_innerType, methodNode, scope, codegenClassScope);
			EnumDistinctOfHelper.ForEachBlock(block, eval, _innerType);
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(ExprDotMethod(Ref("distinct"), "values"));
		}
	}
} // end of namespace
