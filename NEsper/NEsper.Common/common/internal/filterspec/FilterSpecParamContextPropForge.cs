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
        private readonly EventPropertyGetterSPI getter;
        private readonly SimpleNumberCoercer numberCoercer;

        public FilterSpecParamContextPropForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            EventPropertyGetterSPI getter,
            SimpleNumberCoercer numberCoercer)
            : base(lookupable, filterOperator)
        {
            this.getter = getter;
            this.numberCoercer = numberCoercer;
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), GetType(), classScope);

            method.Block
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable), "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(filterOperator));

            var param = NewAnonymousClass(
                method.Block, typeof(FilterSpecParam), CompatExtensions.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            param.AddMethod("getFilterValue", getFilterValue);
            getFilterValue.Block
                .DeclareVar(typeof(EventBean), "props", ExprDotMethod(REF_EXPREVALCONTEXT, "getContextProperties"))
                .IfRefNullReturnNull(Ref("props"))
                .DeclareVar(typeof(object), "result", getter.EventBeanGetCodegen(Ref("props"), method, classScope));
            if (numberCoercer != null) {
                getFilterValue.Block.AssignRef(
                    "result",
                    numberCoercer.CoerceCodegenMayNullBoxed(
                        Cast(typeof(object), Ref("result")), typeof(object), method, classScope));
            }

            getFilterValue.Block.MethodReturn(Ref("result"));

            method.Block.MethodReturn(param);
            return method;
        }
    }
} // end of namespace