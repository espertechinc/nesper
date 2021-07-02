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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby
{
	public class EnumOrderByEventPlus : ThreeFormEventPlus
	{
		private readonly bool _descending;
		private readonly Type _innerBoxedType;

		public EnumOrderByEventPlus(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType indexEventType,
			int numParameters,
			bool descending) : base(lambda, indexEventType, numParameters)
		{
			_descending = descending;
			_innerBoxedType = InnerExpression.EvaluationType.GetBoxedType();
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
						
						var sort = new OrderedListDictionary<object, ICollection<EventBean>>();
						var hasColl = false;
						var indexEvent = new ObjectArrayEventBean(new object[2], FieldEventType);
						
						var props = indexEvent.Properties;
						props[1] = enumcoll.Count;
						
						eventsLambda[StreamNumLambda + 1] = indexEvent;
						var count = -1;
						var beans = (ICollection<EventBean>) enumcoll;

						foreach (EventBean next in beans) {
							count++;
							props[0] = count;
							eventsLambda[StreamNumLambda] = next;

							var comparable = inner.Evaluate(eventsLambda, isNewData, context);
							
							var entry = sort.Get(comparable);
							if (entry == null) {
								entry = new ArrayDeque<EventBean>();
								entry.Add(next);
								sort[comparable] = entry;
								continue;
							}

							entry.Add(next);
							hasColl = true;
						}

						return EnumOrderByHelper.EnumOrderBySortEval(sort, hasColl, _descending);
					},
				};
			}
		}

		public override Type ReturnTypeOfMethod()
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
				.DeclareVar<OrderedListDictionary<object, ICollection<EventBean>>>("sort", NewInstance(typeof(OrderedListDictionary<object, ICollection<EventBean>>)))
				.DeclareVar<bool?>("hasColl", ConstantFalse());
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			EnumOrderByHelper.SortingCode<EventBean>(block, _innerBoxedType, InnerExpression, methodNode, scope, codegenClassScope);
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(
				FlexWrap(
					StaticMethod(
						typeof(EnumOrderByHelper),
						"EnumOrderBySortEval",
						Ref("sort"),
						Ref("hasColl"),
						Constant(_descending))));
		}
	}
} // end of namespace
