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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.orderby
{
	public class EnumOrderByHelper
	{
		/// <summary>
		/// NOTE: Code-generation-invoked method, method name and parameter order matters
		/// </summary>
		/// <param name="sort">sorted</param>
		/// <param name="hasColl">collection flag</param>
		/// <param name="descending">true for descending</param>
		/// <returns>collection</returns>
		public static ICollection<object> EnumOrderBySortEval(
			IOrderedDictionary<object, object> sort,
			bool hasColl,
			bool descending)
		{
			IDictionary<object, object> sorted;
			if (descending) {
				sorted = sort.Invert();
			}
			else {
				sorted = sort;
			}

			if (!hasColl) {
				return sorted.Values;
			}

			var coll = new ArrayDeque<object>();
			foreach (var entry in sorted) {
				if (entry.Value.IsObjectCollectionCompatible()) {
					coll.AddAll(entry.Value.AsObjectCollection());
				}
				else {
					coll.Add(entry.Value);
				}
			}

			return coll;
		}

		public static void SortingCode(
			CodegenBlock block,
			Type innerBoxedType,
			ExprForge innerExpression,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			block
				.DeclareVar(innerBoxedType, "value", innerExpression.EvaluateCodegen(innerBoxedType, methodNode, scope, codegenClassScope))
				.DeclareVar<object>("entry", ExprDotMethod(Ref("sort"), "Get", Ref("value")))
				.IfCondition(EqualsNull(Ref("entry")))
				.Expression(ExprDotMethod(Ref("sort"), "Put", Ref("value"), Ref("next")))
				.BlockContinue()
				.IfCondition(InstanceOf(Ref("entry"), typeof(ICollection<object>)))
				.ExprDotMethod(Cast(typeof(ICollection<object>), Ref("entry")), "Add", Ref("next"))
				.BlockContinue()
				.DeclareVar<Deque<object>>("coll", NewInstance(typeof(ArrayDeque<object>), Constant(2)))
				.ExprDotMethod(Ref("coll"), "Add", Ref("entry"))
				.ExprDotMethod(Ref("coll"), "Add", Ref("next"))
				.ExprDotMethod(Ref("sort"), "Put", Ref("value"), Ref("coll"))
				.AssignRef("hasColl", ConstantTrue())
				.BlockEnd();
		}
	}
} // end of namespace
