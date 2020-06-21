///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.codegen;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
	public class EnumFirstOfEvent : EnumForgeBasePlain
	{
		public EnumFirstOfEvent(ExprDotEvalParamLambda lambda) : base(lambda)
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
						foreach (var next in beans) {
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

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams premade,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var scope = new ExprForgeCodegenSymbol(false, null);
			var methodNode = codegenMethodScope.MakeChildWithScope(typeof(EventBean), typeof(EnumFirstOfEvent), scope, codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);

			var block = methodNode.Block;
			var forEach = block
				.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(StreamNumLambda), Ref("next"));
			CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
				forEach,
				InnerExpression.EvaluationType,
				InnerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
			forEach.BlockReturn(Ref("next"));
			block.MethodReturn(ConstantNull());
			return LocalMethod(methodNode, premade.Eps, premade.Enumcoll, premade.IsNewData, premade.ExprCtx);
		}
	}
} // end of namespace
