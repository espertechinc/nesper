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
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.@event.arr;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // Ref;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
	public class EnumSumScalar : ThreeFormScalar
	{
		private readonly ExprDotEvalSumMethodFactory _sumMethodFactory;

		public EnumSumScalar(
			ExprDotEvalParamLambda lambda,
			ObjectArrayEventType fieldEventType,
			int numParameters,
			ExprDotEvalSumMethodFactory sumMethodFactory) : base(lambda, fieldEventType, numParameters)
		{
			this._sumMethodFactory = sumMethodFactory;
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
						var method = _sumMethodFactory.SumAggregator;

						var resultEvent = new ObjectArrayEventBean(new object[3], fieldEventType);
						eventsLambda[StreamNumLambda] = resultEvent;
						var props = resultEvent.Properties;
						props[2] = enumcoll.Count;

						var count = -1;
						var values = (ICollection<object>) enumcoll;
						foreach (var next in values) {
							count++;
							props[0] = next;
							props[1] = count;

							var value = inner.Evaluate(eventsLambda, isNewData, context);
							method.Enter(value);
						}

						return method.Value;
					},
				};
			}
		}

		public override Type ReturnTypeOfMethod() {
	        return _sumMethodFactory.ValueType;
	    }

	    public override CodegenExpression ReturnIfEmptyOptional() {
	        return null;
	    }

	    public override void InitBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        _sumMethodFactory.CodegenDeclare(block);
	    }

	    public override void ForEachBlock(CodegenBlock block, CodegenMethod methodNode, ExprForgeCodegenSymbol scope, CodegenClassScope codegenClassScope) {
	        var innerType = InnerExpression.EvaluationType;
	        block.DeclareVar(innerType, "value", InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope));
	        if (!innerType.IsPrimitive) {
	            block.IfRefNull("value").BlockContinue();
	        }
	        _sumMethodFactory.CodegenEnterNumberTypedNonNull(block, Ref("value"));
	    }

	    public override void ReturnResult(CodegenBlock block) {
	        _sumMethodFactory.CodegenReturn(block);
	    }
	}
} // end of namespace
