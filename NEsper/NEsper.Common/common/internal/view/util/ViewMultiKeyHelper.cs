///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.view.util
{
	public class ViewMultiKeyHelper
	{
		public static void Assign(
			ExprNode[] criteriaExpressions,
			MultiKeyClassRef multiKeyClassNames,
			CodegenMethod method,
			CodegenExpressionRef factory,
			SAIFFInitializeSymbol symbols,
			CodegenClassScope classScope)
		{
			CodegenExpression criteriaEval = MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(criteriaExpressions, null, multiKeyClassNames, method, classScope);
			method.Block
				.SetProperty(factory, "CriteriaEval", criteriaEval)
				.SetProperty(factory, "CriteriaTypes", Constant(ExprNodeUtilityQuery.GetExprResultTypes(criteriaExpressions)))
				.SetProperty(factory, "KeySerde", multiKeyClassNames.GetExprMKSerde(method, classScope));
		}
	}
} // end of namespace
