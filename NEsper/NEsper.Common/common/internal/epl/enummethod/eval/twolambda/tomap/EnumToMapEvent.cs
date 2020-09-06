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
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.twolambda.tomap
{
	public class EnumToMapEvent : EnumForgeBasePlain
	{
		private readonly ExprForge secondExpression;

		public ExprForge SecondExpression => secondExpression;

		public EnumToMapEvent(
			ExprForge innerExpression,
			int streamCountIncoming,
			ExprForge secondExpression) : base(innerExpression, streamCountIncoming)
		{
			this.secondExpression = secondExpression;
		}

		public override EnumEval EnumEvaluator {
			get {
				var first = InnerExpression.ExprEvaluator;
				var second = secondExpression.ExprEvaluator;
				return new ProxyEnumEval(
					(
						eventsLambda,
						enumcoll,
						isNewData,
						context) => {
						if (enumcoll.IsEmpty()) {
							return EmptyDictionary<object, object>.Instance;
						}

						IDictionary<object, object> map = new NullableDictionary<object, object>();

						var beans = (ICollection<EventBean>) enumcoll;
						foreach (var next in beans) {
							eventsLambda[StreamNumLambda] = next;

							var key = first.Evaluate(eventsLambda, isNewData, context);
							var value = second.Evaluate(eventsLambda, isNewData, context);
							map.Put(key, value);
						}

						return map;
					});
			}
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams premade,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var scope = new ExprForgeCodegenSymbol(false, null);
			var methodNode = codegenMethodScope
				.MakeChildWithScope(typeof(IDictionary<object, object>), typeof(EnumToMapEvent), scope, codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);

			var block = methodNode.Block
				.IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty"))
				.BlockReturn(EnumValue(typeof(EmptyDictionary<object, object>), "Instance"));
			block.DeclareVar(typeof(IDictionary<object, object>), "map", NewInstance(typeof(NullableDictionary<object, object>)));
			block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("next"))
				.DeclareVar<object>("key", InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
				.DeclareVar<object>("value", secondExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope))
				.Expression(ExprDotMethod(Ref("map"), "Put", Ref("key"), Ref("value")));
			block.MethodReturn(Ref("map"));
			return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
		}
	}
} // end of namespace
