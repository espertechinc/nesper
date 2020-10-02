///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.firstoflastof
{
	public class EnumFirstOf : EnumForgeBasePlain,
		EnumForge,
		EnumEval
	{
		private readonly EPType _resultType;

		public EnumFirstOf(
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
			if (enumcoll == null || enumcoll.IsEmpty()) {
				return null;
			}

			return enumcoll.First();
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var type = _resultType.GetCodegenReturnType();
			var method = codegenMethodScope
				.MakeChild(type, typeof(EnumFirstOf), codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS)
				.Block
				.IfCondition(Or(EqualsNull(EnumForgeCodegenNames.REF_ENUMCOLL), ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty")))
				.BlockReturn(ConstantNull())
				.Debug("M5 => {0}", EnumForgeCodegenNames.REF_ENUMCOLL)
				.MethodReturn(FlexCast(type, ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("First")));
			return LocalMethod(method, args.Expressions);
		}
	}
} // end of namespace
