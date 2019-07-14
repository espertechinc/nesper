///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    public abstract class FilterSpecParamForge
    {
        public static readonly FilterSpecParamForge[] EMPTY_PARAM_ARRAY = new FilterSpecParamForge[0];

        internal readonly FilterOperator filterOperator;

        /// <summary>
        ///     The property name of the filter parameter.
        /// </summary>
        internal readonly ExprFilterSpecLookupableForge lookupable;

        protected FilterSpecParamForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator)
        {
            this.lookupable = lookupable;
            this.filterOperator = filterOperator;
        }

        public ExprFilterSpecLookupableForge Lookupable => lookupable;

        public FilterOperator FilterOperator => filterOperator;

        public abstract CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols);

        public static FilterSpecParamForge[] ToArray(ICollection<FilterSpecParamForge> coll)
        {
            if (coll.IsEmpty()) {
                return EMPTY_PARAM_ARRAY;
            }

            return coll.ToArray();
        }

        public static CodegenMethod MakeParamArrayArrayCodegen(
            FilterSpecParamForge[][] forges,
            CodegenClassScope classScope,
            CodegenMethod parent)
        {
            var symbolsWithType = new SAIFFInitializeSymbolWEventType();
            var method = parent
                .MakeChildWithScope(
                    typeof(FilterSpecParam[][]),
                    typeof(FilterSpecParamForge),
                    symbolsWithType,
                    classScope)
                .AddParam(typeof(EventType), SAIFFInitializeSymbolWEventType.REF_EVENTTYPE.Ref)
                .AddParam(
                    typeof(EPStatementInitServices),
                    SAIFFInitializeSymbol.REF_STMTINITSVC.Ref);
            method.Block.DeclareVar(
                typeof(FilterSpecParam[][]),
                "params",
                NewArrayByLength(typeof(FilterSpecParam[]), Constant(forges.Length)));

            for (var i = 0; i < forges.Length; i++) {
                method.Block.AssignArrayElement(
                    "params",
                    Constant(i),
                    LocalMethod(MakeParamArrayCodegen(forges[i], classScope, method, symbolsWithType)));
            }

            method.Block.MethodReturn(Ref("params"));
            return method;
        }

        private static CodegenMethod MakeParamArrayCodegen(
            FilterSpecParamForge[] forges,
            CodegenClassScope classScope,
            CodegenMethod parent,
            SAIFFInitializeSymbolWEventType symbolsWithType)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam[]), typeof(FilterSpecParamForge), classScope);
            method.Block.DeclareVar(
                typeof(FilterSpecParam[]),
                "items",
                NewArrayByLength(typeof(FilterSpecParam), Constant(forges.Length)));
            for (var i = 0; i < forges.Length; i++) {
                var makeParam = forges[i].MakeCodegen(classScope, method, symbolsWithType);
                method.Block.AssignArrayElement("items", Constant(i), LocalMethod(makeParam));
            }

            method.Block.MethodReturn(Ref("items"));
            return method;
        }
    }
} // end of namespace