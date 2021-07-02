///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.distinctof
{
	public class EnumDistinctOfHelper
	{
		public static void ForEachBlock(
			CodegenBlock block,
			CodegenExpression eval,
			Type innerType)
		{
			if (innerType.IsNullType()) {
				block.DeclareVar<object>("comparable", ConstantNull());
			}
			else if (!innerType.IsArray) {
				block.DeclareVar(innerType, "comparable", eval);
			}
			else {
				var componentType = innerType.GetElementType();
				var arrayMK = MultiKeyPlanner.GetMKClassForComponentType(componentType);
				block.DeclareVar(arrayMK, "comparable", NewInstance(arrayMK, eval));
			}

			block.IfCondition(Not(ExprDotMethod(Ref("distinct"), "ContainsKey", Ref("comparable"))))
				.Expression(ExprDotMethod(Ref("distinct"), "Put", Ref("comparable"), Ref("next")))
				.BlockEnd();
		}
	}
} // end of namespace
