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
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational; // GE
using static com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile.EnumTakeWhileHelper; // takeWhileLastEventBeanToArray

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile
{
	public class EnumTakeWhileLastEvent : ThreeFormEventPlain
	{

		private CodegenExpression innerValue;

		public EnumTakeWhileLastEvent(ExprDotEvalParamLambda lambda) : base(lambda)
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

						var eventBeanCollection = enumcoll.Unwrap<EventBean>();
						if (enumcoll.Count == 1) {
							var item = eventBeanCollection.First();
							eventsLambda[StreamNumLambda] = item;

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || (!(Boolean) pass)) {
								return EmptyList<object>.Instance;
							}

							return Collections.SingletonList(item);
						}

						var all = TakeWhileLastEventBeanToArray(eventBeanCollection);
						var result = new ArrayDeque<object>();

						for (var i = all.Length - 1; i >= 0; i--) {
							eventsLambda[StreamNumLambda] = all[i];

							var pass = inner.Evaluate(eventsLambda, isNewData, context);
							if (pass == null || (!(Boolean) pass)) {
								break;
							}

							result.AddFirst(all[i]);
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
			var blockSingle = block.IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
				.DeclareVar(
					typeof(EventBean),
					"item",
					Cast(typeof(EventBean), ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("iterator").Add("next")))
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("item"));
			CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
				blockSingle,
				InnerExpression.EvaluationType,
				innerValue,
				StaticMethod(typeof(Collections), "emptyList"));
			blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", Ref("item")));

			block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)))
				.DeclareVar(
					typeof(EventBean[]),
					"all",
					StaticMethod(typeof(EnumTakeWhileHelper), "takeWhileLastEventBeanToArray", EnumForgeCodegenNames.REF_ENUMCOLL));

			var forEach = block.ForLoop(
					typeof(int),
					"i",
					Op(ArrayLength(Ref("all")), "-", Constant(1)),
					Relational(Ref("i"), GE, Constant(0)),
					DecrementRef("i"))
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), ArrayAtIndex(Ref("all"), Ref("i")));
			CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(forEach, InnerExpression.EvaluationType, innerValue);
			forEach.Expression(ExprDotMethod(Ref("result"), "addFirst", ArrayAtIndex(Ref("all"), Ref("i"))));
		}

		public override bool HasForEachLoop()
		{
			return false;
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			throw new IllegalStateException();
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(Ref("result"));
		}
	}
} // end of namespace
