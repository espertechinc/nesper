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
		public static ICollection<T> EnumOrderBySortEval<T>(
			IOrderedDictionary<object, ICollection<T>> sort,
			bool hasColl,
			bool descending)
		{
			IDictionary<object, ICollection<T>> sorted = descending ? sort.Invert() : sort;

			// if (!hasColl) {
			// 	return sorted.Values;
			// }

			var coll = new ArrayDeque<T>();
			foreach (var entry in sorted) {
				coll.AddAll(entry.Value);
			}

			return coll;
		}

		public static void SortingCode<T>(
			CodegenBlock block,
			Type innerBoxedType,
			ExprForge innerExpression,
			CodegenMethod methodNode,
			ExprForgeCodegenSymbol scope,
			CodegenClassScope codegenClassScope)
		{
			var componentType = typeof(T);
			var collectionType = typeof(ICollection<>).MakeGenericType(componentType);
			var dequeType = typeof(ArrayDeque<>).MakeGenericType(componentType);
			
			block
				.DeclareVar(innerBoxedType, "value", innerExpression.EvaluateCodegen(innerBoxedType, methodNode, scope, codegenClassScope))
				.DeclareVar(collectionType, "entry", ExprDotMethod(Ref("sort"), "Get", Ref("value")))
				
				.IfCondition(EqualsNull(Ref("entry")))
				.AssignRef("entry", NewInstance(dequeType))
				.ExprDotMethod(Ref("entry"), "Add", Ref("next"))
				.Expression(ExprDotMethod(Ref("sort"), "Put", Ref("value"), Ref("entry")))
				.BlockContinue()
				
				.ExprDotMethod(Ref("entry"), "Add", Ref("next"))
				.BlockEnd();
		}
	}
} // end of namespace
