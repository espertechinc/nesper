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
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational; // LE

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof
{
	public class EnumDistinctOfScalarNoParams : EnumForgeBasePlain,
		EnumForge,
		EnumEval
	{
		private readonly Type _fieldType;

		public EnumDistinctOfScalarNoParams(
			int streamCountIncoming,
			Type fieldType) : base(streamCountIncoming)
		{
			_fieldType = fieldType;
		}

		public override EnumEval EnumEvaluator => this;

		public object EvaluateEnumMethod(
			EventBean[] eventsLambda,
			ICollection<object> enumcoll,
			bool isNewData,
			ExprEvaluatorContext context)
		{
			if (enumcoll.Count <= 1) {
				return enumcoll;
			}

			if (enumcoll is ISet<object>) {
				return enumcoll;
			}

			return new HashSet<object>(enumcoll);
		}

		public override CodegenExpression Codegen(
			EnumForgeCodegenParams args,
			CodegenMethodScope codegenMethodScope,
			CodegenClassScope codegenClassScope)
		{
			var method = codegenMethodScope
				.MakeChild(typeof(ICollection<object>), typeof(EnumDistinctOfScalarNoParams), codegenClassScope)
				.AddParam(EnumForgeCodegenNames.PARAMS);

			method.Block
				.IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
				.BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);

			if (_fieldType == null || !_fieldType.IsArray) {
				method.Block
					.IfCondition(InstanceOf(Ref("enumcoll"), typeof(ISet<object>)))
					.BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
					.MethodReturn(NewInstance(typeof(LinkedHashSet<object>), EnumForgeCodegenNames.REF_ENUMCOLL));
			}
			else {
				var arrayMK = MultiKeyPlanner.GetMKClassForComponentType(_fieldType.GetElementType());
				method.Block.DeclareVar<IDictionary<object, object>>("distinct", NewInstance(typeof(LinkedHashMap<object, object>)));
				
				var loop = method.Block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL);
				loop.DeclareVar(arrayMK, "comparable", NewInstance(arrayMK, Cast(_fieldType, Ref("next"))))
					.Expression(ExprDotMethod(Ref("distinct"), "Put", Ref("comparable"), Ref("next")));
				
				method.Block.MethodReturn(ExprDotName(Ref("distinct"), "Values"));
			}

			return LocalMethod(method, args.Expressions);
		}
	}
} // end of namespace
