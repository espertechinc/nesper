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
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents a single, constant value filter parameter in an <seealso cref="FilterSpecActivatable" />
    ///     filter specification.
    /// </summary>
    public sealed class FilterSpecParamConstantForge : FilterSpecParamForge
    {
        private readonly object _filterConstant;

        public FilterSpecParamConstantForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            object filterConstant)
            : base(lookupable, filterOperator)
        {
            this._filterConstant = filterConstant;

            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " + filterOperator + " supplied to " +
                    "constant filter parameter");
            }
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <returns>constant value</returns>
        public object FilterConstant => _filterConstant;

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);
            method.Block
                .DeclareVar(typeof(ExprFilterSpecLookupable), "lookupable", LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(typeof(FilterOperator), filterOperator.GetName()));

            var inner = NewAnonymousClass(
                method.Block, typeof(FilterSpecParam),
                CompatExtensions.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope).AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            inner.AddMethod("getFilterValue", getFilterValue);
            getFilterValue.Block.MethodReturn(Constant(_filterConstant));

            method.Block.MethodReturn(inner);
            return method;
        }

        public override string ToString()
        {
            return base.ToString() + " filterConstant=" + _filterConstant;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            if (!base.Equals(o)) {
                return false;
            }

            var that = (FilterSpecParamConstantForge) o;

            if (_filterConstant != null ? !_filterConstant.Equals(that._filterConstant) : that._filterConstant != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + (_filterConstant != null ? _filterConstant.GetHashCode() : 0);
            return result;
        }
    }
} // end of namespace