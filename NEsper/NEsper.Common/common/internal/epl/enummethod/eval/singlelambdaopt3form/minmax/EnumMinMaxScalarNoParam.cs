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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.minmax
{
	public class EnumMinMaxScalarNoParam : EnumForgeBasePlain,
		EnumForge,
		EnumEval
	{
		private readonly bool _max;
		private readonly EPType _resultType;

		public EnumMinMaxScalarNoParam(
			int streamCountIncoming,
			bool max,
			EPType resultType) : base(streamCountIncoming)
		{
			this._max = max;
			this._resultType = resultType;
		}

		public override EnumEval EnumEvaluator => this;

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			IComparable minKey = null;

			foreach (object next in enumcoll) {

				object comparable = next;
				if (comparable == null) {
					continue;
				}

				if (minKey == null) {
					minKey = (IComparable) comparable;
				}
				else {
					if (_max) {
						if (minKey.CompareTo(comparable) < 0) {
							minKey = (IComparable) comparable;
						}
					}
					else {
						if (minKey.CompareTo(comparable) > 0) {
							minKey = (IComparable) comparable;
						}
					}
				}
			}

			return minKey;
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			Type innerTypeBoxed = Boxing.GetBoxedType(EPTypeHelper.GetCodegenReturnType(_resultType));

			CodegenBlock block = codegenMethodScope
				.MakeChild(innerTypeBoxed, typeof(EnumMinMaxScalarNoParam), codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS)
				.Block
				.DeclareVar(innerTypeBoxed, "minKey", ConstantNull());

			CodegenBlock forEach = block
				.ForEach(innerTypeBoxed, "value", EnumForgeCodegenNames.REF_ENUMCOLL)
				.IfRefNull("value")
				.BlockContinue();

			var compareTo =
				StaticMethod(
					typeof(SmartCompare),
					"Compare",
					Ref("minKey"),
					Ref("value"));
			
			forEach
				.IfCondition(EqualsNull(Ref("minKey")))
				.AssignRef("minKey", FlexCast(innerTypeBoxed, Ref("value")))
				.IfElse()
				.IfCondition(Relational(compareTo, _max ? LT : GT, Constant(0)))
				.AssignRef("minKey", FlexCast(innerTypeBoxed, Ref("value")));

			CodegenMethod method = block.MethodReturn(Ref("minKey"));
			return LocalMethod(method, args.Expressions);
		}
	}
} // end of namespace
