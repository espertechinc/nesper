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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby
{
	public class EnumOrderByScalarNoParams : EnumForgeBasePlain,
		EnumEval
	{
		private readonly bool _descending;

		public EnumOrderByScalarNoParams(
			int streamCountIncoming,
			bool descending) : base(streamCountIncoming)
		{
			_descending = descending;
		}

		public override EnumEval EnumEvaluator {
			get { return this; }
		}

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{

			if (enumcoll == null || enumcoll.IsEmpty()) {
				return enumcoll;
			}

			var list = new List<object>(enumcoll);
			list.Sort();
			if (_descending) {
				list.Reverse();
			}

			return list;
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var block = codegenMethodScope
				.MakeChild(typeof(ICollection<object>), typeof(EnumOrderByScalarNoParams), codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS)
				.Block
				.IfCondition(Or(EqualsNull(EnumForgeCodegenNames.REF_ENUMCOLL), ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "IsEmpty")))
				.BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
				.DeclareVar<List<object>>("list", NewInstance(typeof(List<object>), EnumForgeCodegenNames.REF_ENUMCOLL));
			if (_descending) {
				block.StaticMethod(typeof(Collections), "SortInPlace", Ref("list"), StaticMethod(typeof(Comparers), "Inverse", new[] {typeof(object)}));
			}
			else {
				block.StaticMethod(typeof(Collections), "SortInPlace", Ref("list"));
			}

			var method = block.MethodReturn(Ref("list"));
			return LocalMethod(method, args.Expressions);
		}
	}
} // end of namespace
