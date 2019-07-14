///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamDeployTimeConstParamForge : FilterSpecParamForge
    {
        private readonly ExprNodeDeployTimeConst _deployTimeConstant;
        private readonly SimpleNumberCoercer _numberCoercer;
        private readonly Type _returnType;

        public FilterSpecParamDeployTimeConstParamForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            ExprNodeDeployTimeConst deployTimeConstant,
            Type returnType,
            SimpleNumberCoercer numberCoercer)
            : base(
                lookupable,
                filterOperator)
        {
            _deployTimeConstant = deployTimeConstant;
            _returnType = returnType;
            _numberCoercer = numberCoercer;
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable),
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(filterOperator));

            var param = NewAnonymousClass(
                method.Block,
                typeof(FilterSpecParam),
                Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            param.AddMethod("getFilterValue", getFilterValue);
            var value = _deployTimeConstant.CodegenGetDeployTimeConstValue(classScope);
            if (_numberCoercer != null) {
                value = _numberCoercer.CoerceCodegenMayNullBoxed(value, _returnType, method, classScope);
            }

            getFilterValue.Block.MethodReturn(value);

            method.Block.MethodReturn(param);
            return method;
        }
    }
} // end of namespace