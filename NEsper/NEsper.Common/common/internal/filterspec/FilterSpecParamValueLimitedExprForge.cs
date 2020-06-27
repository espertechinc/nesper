///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames; // REF_EXPREVALCONTEXT;

namespace com.espertech.esper.common.@internal.filterspec
{
	public class FilterSpecParamValueLimitedExprForge : FilterSpecParamForge
	{
		private readonly ExprNode _value;
		private readonly MatchedEventConvertorForge _convertor;
		private readonly Coercer _numberCoercer;

		public FilterSpecParamValueLimitedExprForge(
			ExprFilterSpecLookupableForge lookupable,
			FilterOperator filterOperator,
			ExprNode value,
			MatchedEventConvertorForge convertor,
			Coercer numberCoercer)
			: base(lookupable, filterOperator)
		{
			this._value = value;
			this._convertor = convertor;
			this._numberCoercer = numberCoercer;
		}

		public override CodegenMethod MakeCodegen(
			CodegenClassScope classScope,
			CodegenMethodScope parent,
			SAIFFInitializeSymbolWEventType symbols)
		{
			var method = parent
				.MakeChild(typeof(FilterSpecParam), GetType(), classScope);
			method.Block
				.DeclareVar<ExprFilterSpecLookupable>(
					"lookupable",
					LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
				.DeclareVar<FilterOperator>("op", EnumValue(typeof(FilterOperator), filterOperator.GetName()));

			var getFilterValue = new CodegenExpressionLambda(method.Block)
				.WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
			var inner = NewInstance<ProxyFilterSpecParam>(
				Ref("lookupable"),
				Ref("op"),
				getFilterValue);

			var rhsExpression = CodegenLegoMethodExpression.CodegenExpression(_value.Forge, method, classScope);
			var matchEventConvertor = _convertor.Make(method, classScope);
			
			CodegenExpression valueExpr = LocalMethod(rhsExpression, Ref("eps"), ConstantTrue(), REF_EXPREVALCONTEXT);
			if (_numberCoercer != null) {
				valueExpr = _numberCoercer.CoerceCodegenMayNullBoxed(valueExpr, _value.Forge.EvaluationType, method, classScope);
			}

			getFilterValue.Block
				.DeclareVar<EventBean[]>("eps", LocalMethod(matchEventConvertor, FilterSpecParam.REF_MATCHEDEVENTMAP))
				.BlockReturn(FilterValueSetParamImpl.CodegenNew(valueExpr));

			method.Block.MethodReturn(inner);
			return method;
		}

		public override void ValueExprToString(
			StringBuilder @out,
			int i)
		{
			@out.Append("expression '").Append(ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(_value)).Append("'");
		}
	}
} // end of namespace
