///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.takewhile
{
	public class EnumTakeWhileHelper
	{

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="enumcoll">events</param>
		/// <returns>array</returns>
		public static EventBean[] TakeWhileLastEventBeanToArray(ICollection<EventBean> enumcoll)
		{
			return enumcoll.ToArray();

#if DEPRECATED
			int size = enumcoll.Count;
			EventBean[] all = new EventBean[size];
			int count = 0;
			foreach (EventBean item in enumcoll) {
				all[count++] = item;
			}

			return all;
#endif
		}

		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="enumcoll">coll</param>
		/// <returns>array</returns>
		public static object[] TakeWhileLastScalarToArray<T>(ICollection<T> enumcoll)
		{
			return enumcoll.Cast<object>().ToArray();

#if DEPRECATED
			int size = enumcoll.Count;
			object[] all = new object[size];
			int count = 0;
			foreach (object item in enumcoll) {
				all[count++] = item;
			}

			return all;
#endif
		}

		public static void InitBlockSizeOneScalar(
			int numParameters,
			CodegenBlock block,
			CodegenExpression innerValue,
			Type evaluationType)
		{
			var blockSingle = block.IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
				.DeclareVar<object>("item", ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("iterator").Add("next"))
				.AssignArrayElement("props", Constant(0), Ref("item"));
			if (numParameters >= 2) {
				blockSingle.AssignArrayElement("props", Constant(1), Constant(0));
			}

			CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
				blockSingle,
				evaluationType,
				innerValue,
				StaticMethod(typeof(Collections), "emptyList"));
			blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", Ref("item")));

			block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)));
		}

		public static void InitBlockSizeOneEvent(
			CodegenBlock block,
			CodegenExpression innerValue,
			int streamNumLambda,
			Type evaluationType)
		{
			var blockSingle = block.IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
				.DeclareVar(
					typeof(EventBean),
					"item",
					Cast(typeof(EventBean), ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL)
						.Add("iterator")
						.Add("next")))
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(streamNumLambda), Ref("item"));
			CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
				blockSingle,
				evaluationType,
				innerValue,
				StaticMethod(typeof(Collections), "emptyList"));
			blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", Ref("item")));

			block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)));
		}

		public static void InitBlockSizeOneEventPlus(
			int numParameters,
			CodegenBlock block,
			CodegenExpression innerValue,
			int streamNumLambda,
			Type evaluationType)
		{
			var blockSingle = block
				.IfCondition(EqualsIdentity(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), Constant(1)))
				.DeclareVar(
					typeof(EventBean),
					"item",
					Cast(typeof(EventBean), ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL)
						.Add("iterator")
						.Add("next")))
				.AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(streamNumLambda), Ref("item"))
				.AssignArrayElement("props", Constant(0), Constant(0));
			if (numParameters > 2) {
				blockSingle.AssignArrayElement("props", Constant(1), Constant(1));
			}

			CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
				blockSingle,
				evaluationType,
				innerValue,
				StaticMethod(typeof(Collections), "emptyList"));
			blockSingle.BlockReturn(
				StaticMethod(typeof(Collections), "singletonList", Ref("item")));

			block.DeclareVar<ArrayDeque<object>>("result", NewInstance(typeof(ArrayDeque<object>)));
		}
	}
} // end of namespace
