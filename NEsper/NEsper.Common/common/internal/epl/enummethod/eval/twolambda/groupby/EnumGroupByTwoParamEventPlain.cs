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
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.enummethodeval.twolambda.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.groupby
{
	public class EnumGroupByTwoParamEventPlain : TwoLambdaThreeFormEventPlain
	{
		public EnumGroupByTwoParamEventPlain(
			ExprForge innerExpression,
			int streamCountIncoming,
			ExprForge secondExpression)
			: base(innerExpression, streamCountIncoming, secondExpression)
		{
		}

		public override EnumEval EnumEvaluator {
			get {
				ExprEvaluator first = InnerExpression.ExprEvaluator;
				ExprEvaluator second = SecondExpression.ExprEvaluator;
				return new ProxyEnumEval(
					(
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
						if (enumcoll.IsEmpty()) {
							return EmptyDictionary<object, ICollection<object>>.Instance;
						}

						IDictionary<object, ICollection<object>> result = new LinkedHashMap<object, ICollection<object>>();

						ICollection<EventBean> beans = (ICollection<EventBean>) enumcoll;
						foreach (EventBean next in beans) {
							eventsLambda[StreamNumLambda] = next;

							object key = first.Evaluate(eventsLambda, isNewData, context);
							object entry = second.Evaluate(eventsLambda, isNewData, context);

							ICollection<object> value = result.Get(key);
							if (value == null) {
								value = new List<object>();
								result.Put(key, value);
							}

							value.Add(entry);
						}

						return result;
					});
			}
		}

		public override Type ReturnType()
		{
			return typeof(IDictionary<string, object>);
		}

		public override CodegenExpression ReturnIfEmptyOptional()
		{
			return StaticMethod(typeof(Collections), "emptyMap");
		}

		public override void InitBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			block.DeclareVar(typeof(IDictionary<string, object>), "result", NewInstance(typeof(Dictionary<string, object>)));
		}

		public override void ForEachBlock(
			CodegenBlock block,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			block.DeclareVar(typeof(object), "key", InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
				.DeclareVar(typeof(object), "entry", SecondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
				.DeclareVar(typeof(ICollection<object>), "value", Cast(typeof(ICollection<object>), ExprDotMethod(Ref("result"), "get", Ref("key"))))
				.IfRefNull("value")
				.AssignRef("value", NewInstance(typeof(List<object>)))
				.Expression(ExprDotMethod(Ref("result"), "put", Ref("key"), Ref("value")))
				.BlockEnd()
				.Expression(ExprDotMethod(Ref("value"), "add", Ref("entry")));
		}

		public override void ReturnResult(CodegenBlock block)
		{
			block.MethodReturn(Ref("result"));
		}
	}
} // end of namespace
