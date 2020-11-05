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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.mostleastfreq.EnumMostLeastFrequentHelper; // getEnumMostLeastFrequentResult

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.mostleastfreq
{
	public class EnumMostLeastFrequentEvent : ThreeFormEventPlain
	{
		private readonly bool _isMostFrequent;
		private readonly Type _returnType;

		public EnumMostLeastFrequentEvent(
			ExprDotEvalParamLambda lambda,
			bool mostFrequent) : base(lambda)
		{
			_isMostFrequent = mostFrequent;
			_returnType = InnerExpression.EvaluationType.GetBoxedType();
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
							return null;
						}

						var items = new Dictionary<object, int>();
						var beans = (ICollection<EventBean>) enumcoll;

						foreach (var next in beans) {
							eventsLambda[StreamNumLambda] = next;

							var item = inner.Evaluate(eventsLambda, isNewData, context);
							items.TryGetValue(item, out var existing);
							existing++;
							items.Put(item, existing);
						}

						return GetEnumMostLeastFrequentResult(items, _isMostFrequent);
					},
				};
			}
		}

		public override Type ReturnType()
		{
			return _returnType;
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
			block.DeclareVar<IDictionary<object, int>>("items", NewInstance(typeof(LinkedHashMap<object, int>)));
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			block.DeclareVar<object>("key", InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
				.DeclareVar<int?>("existing", ExprDotMethod(ExprDotMethod(Ref("items"), "Get", Ref("key")), "AsBoxedInt32"))
				.IfCondition(EqualsNull(Ref("existing")))
				.AssignRef("existing", Constant(1))
				.IfElse()
				.IncrementRef("existing")
				.BlockEnd()
				.ExprDotMethod(Ref("items"), "Put", Ref("key"), Unbox(Ref("existing")));
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(
				FlexCast(
					_returnType,
					StaticMethod(typeof(EnumMostLeastFrequentHelper), "GetEnumMostLeastFrequentResult", Ref("items"), Constant(_isMostFrequent))));
		}
	}
} // end of namespace
