///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.filterspec
{
    /// <summary>
    ///     This class represents a range filter parameter in an <seealso cref="FilterSpecActivatable" /> filter specification.
    /// </summary>
    public sealed class FilterSpecParamRangeForge : FilterSpecParamForge
    {
        private readonly FilterSpecParamFilterForEvalForge _max;
        private readonly FilterSpecParamFilterForEvalForge _min;

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="lookupable">is the lookupable</param>
        /// <param name="filterOperator">is the type of range operator</param>
        /// <param name="min">is the begin point of the range</param>
        /// <param name="max">is the end point of the range</param>
        /// <throws>ArgumentException if an operator was supplied that does not take a double range value</throws>
        public FilterSpecParamRangeForge(
            ExprFilterSpecLookupableForge lookupable, FilterOperator filterOperator,
            FilterSpecParamFilterForEvalForge min, FilterSpecParamFilterForEvalForge max)
            : base(lookupable, filterOperator)
        {
            this._min = min;
            this._max = max;

            if (!filterOperator.IsRangeOperator() && !filterOperator.IsInvertedRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " + filterOperator + " supplied to " +
                    "range filter parameter");
            }
        }

        public override string ToString()
        {
            return base.ToString() + "  range=(min=" + _min + ",max=" + _max + ')';
        }

        private bool Equals(FilterSpecParamRangeForge other)
        {
            return Equals(_max, other._max) && Equals(_min, other._min);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            return obj is FilterSpecParamRangeForge && Equals((FilterSpecParamRangeForge) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return ((_max != null ? _max.GetHashCode() : 0) * 397) ^ (_min != null ? _min.GetHashCode() : 0);
            }
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope, CodegenMethodScope parent, SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);
            method.Block
                .DeclareVar(
                    typeof(ExprFilterSpecLookupable), "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar(typeof(FilterOperator), "op", EnumValue(typeof(FilterOperator), filterOperator.GetName()));

            var param = NewAnonymousClass(
                method.Block, typeof(FilterSpecParam), Arrays.AsList(Ref("lookupable"), Ref("op")));
            var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
                .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            param.AddMethod("getFilterValue", getFilterValue);

            var returnType = typeof(DoubleRange);
            var castType = typeof(double?);
            if (lookupable.ReturnType == typeof(string)) {
                castType = typeof(string);
                returnType = typeof(StringRange);
            }

            getFilterValue.Block
                .DeclareVar(typeof(object), "min", _min.MakeCodegen(classScope, method))
                .DeclareVar(typeof(object), "max", _max.MakeCodegen(classScope, method))
                .MethodReturn(NewInstance(returnType, Cast(castType, Ref("min")), Cast(castType, Ref("max"))));

            method.Block.MethodReturn(param);
            return method;
        }
    }
} // end of namespace