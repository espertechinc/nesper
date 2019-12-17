///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;

namespace com.espertech.esper.common.@internal.filterspec
{
    public class FilterSpecParamContextPropForge : FilterSpecParamForge
    {
        private readonly EventPropertyGetterSPI _getter;
        private readonly Coercer _numberCoercer;

        public FilterSpecParamContextPropForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            EventPropertyGetterSPI getter,
            Coercer numberCoercer)
            : base(lookupable, filterOperator)
        {
            _getter = getter;
            _numberCoercer = numberCoercer;
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);

            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("op", EnumValue(filterOperator));

            //var param = NewAnonymousClass(
            //    method.Block,
            //    typeof(FilterSpecParam),
            //    Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("op"),
                getFilterValue);

            //var getFilterValue = CodegenMethod
            //    .MakeParentNode(typeof(object), GetType(), classScope)
            //    .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            //param.AddMethod("GetFilterValue", getFilterValue);

            getFilterValue.Block
                .DeclareVar<EventBean>("props", ExprDotName(REF_EXPREVALCONTEXT, "ContextProperties"))
                .IfRefNullReturnNull(Ref("props"))
                .DeclareVar<object>("result", _getter.EventBeanGetCodegen(Ref("props"), method, classScope));
            if (_numberCoercer != null) {
                getFilterValue.Block.AssignRef(
                    "result",
                    _numberCoercer.CoerceCodegenMayNullBoxed(
                        Cast(typeof(object), Ref("result")),
                        typeof(object),
                        method,
                        classScope));
            }

            getFilterValue.Block.BlockReturn(Ref("result"));

            method.Block.MethodReturn(param);
            return method;
        }
    }
} // end of namespace