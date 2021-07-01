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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
	public class EnumLastOf : EnumForgeBasePlain,
		EnumForge,
		EnumEval
	{
		private readonly EPType _resultType;

		public EnumLastOf(
			int streamCountIncoming,
			EPType resultType) : base(streamCountIncoming)
		{
			this._resultType = resultType;
		}

		public override EnumEval EnumEvaluator => this;

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			object result = null;
			foreach (object next in enumcoll) {
				result = next;
			}

			return result;
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var type = _resultType.GetCodegenReturnType().GetBoxedType();
			var method = codegenMethodScope.MakeChild(type, typeof(EnumLastOf), codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS)
				.Block
				.DeclareVar<object>("result", ConstantNull())
				.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
				.AssignRef("result", Ref("next"))
				.BlockEnd()
				.MethodReturn(FlexCast(type, Ref("result")));
			return LocalMethod(method, args.Expressions);
		}
	}
} // end of namespace
