///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.collections.btree;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby
{
	public class EnumOrderByScalar : ThreeFormScalar
	{
		private readonly bool descending;
		private readonly Type innerBoxedType;

		public EnumOrderByScalar(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType fieldEventType,
			int numParameters,
			bool descending) : base(lambda, fieldEventType, numParameters)
		{
			this.descending = descending;
			innerBoxedType = InnerExpression.EvaluationType.GetBoxedType();
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
						var sort = new OrderedListDictionary<object, ICollection<object>>();
						var hasColl = false;

						var resultEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
						eventsLambda[StreamNumLambda] = resultEvent;
						var props = resultEvent.Properties;
						props[2] = enumcoll.Count;
						var values = (ICollection<object>) enumcoll;

						var count = -1;
						foreach (var next in values) {
							count++;
							props[1] = count;
							props[0] = next;

							var comparable = (IComparable) inner.Evaluate(eventsLambda, isNewData, context);
							var entry = sort.Get(comparable);
							if (entry == null) {
								entry = new ArrayDeque<object>();
								entry.Add(next);
								sort.Put(comparable, entry);
								continue;
							}

							entry.Add(next);
							hasColl = true;
						}

						return EnumOrderByHelper.EnumOrderBySortEval(sort, hasColl, descending);
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
			return null;
		}

		public override void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			block
				.DeclareVar<IOrderedDictionary<object, ICollection<object>>>("sort", NewInstance(typeof(OrderedListDictionary<object, ICollection<object>>)))
				.DeclareVar<bool>("hasColl", ConstantFalse());
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			EnumOrderByHelper.SortingCode<object>(block, innerBoxedType, InnerExpression, methodNode, scope, codegenClassScope);
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(FlexWrap(StaticMethod(typeof(EnumOrderByHelper), "EnumOrderBySortEval", Ref("sort"), Ref("hasColl"), Constant(descending))));
		}
	}
} // end of namespace
