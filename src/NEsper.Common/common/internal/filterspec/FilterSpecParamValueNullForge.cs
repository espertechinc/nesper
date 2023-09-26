///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamValueNullForge : FilterSpecParamForge
    {
        public FilterSpecParamValueNullForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator)
            : base(lookupable, filterOperator)
        {
        }

        public override CodegenExpression MakeCodegen(
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
                .DeclareVar<FilterOperator>(
                    "filterOperator",
                    EnumValue(typeof(FilterOperator), filterOperator.GetName()));

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            var inner = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("filterOperator"),
                getFilterValue);

            getFilterValue
                .Block
                .BlockReturn(FilterValueSetParamImpl.CodegenNew(ConstantNull()));

            method.Block.MethodReturn(inner);
            return LocalMethod(method);
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("(no value expression)");
        }
    }
} // end of namespace