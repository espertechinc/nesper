///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

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
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            FilterSpecParamFilterForEvalForge min,
            FilterSpecParamFilterForEvalForge max)
            : base(lookupable, filterOperator)
        {
            _min = min;
            _max = max;

            if (!filterOperator.IsRangeOperator() && !filterOperator.IsInvertedRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
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

            return obj is FilterSpecParamRangeForge forge && Equals(forge);
        }

        public override int GetHashCode()
        {
            unchecked {
                var result = base.GetHashCode();
                result = result * 31 + (_min != null ? _min.GetHashCode() : 0);
                result = result * 31 + (_max != null ? _max.GetHashCode() : 0);
                return result;
            }
        }

        public override CodegenExpression MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent.MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);
            method.Block
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    LocalMethod(lookupable.MakeCodegen(method, symbols, classScope)))
                .DeclareVar<FilterOperator>("filterOperator", EnumValue(filterOperator));

            var getFilterValue = new CodegenExpressionLambda(method.Block)
                .WithParams(FilterSpecParam.GET_FILTER_VALUE_FP);
            var param = NewInstance<ProxyFilterSpecParam>(
                Ref("lookupable"),
                Ref("filterOperator"),
                getFilterValue);

            //var param = NewAnonymousClass(
            //    method.Block,
            //    typeof(FilterSpecParam),
            //    Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("filterOperator")));
            //var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
            //    .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            //param.AddMethod("GetFilterValue", getFilterValue);

            var returnType = typeof(DoubleRange);
            var castType = typeof(double?);
            if (lookupable.ReturnType == typeof(string)) {
                castType = typeof(string);
                returnType = typeof(StringRange);
            }

            getFilterValue.Block
                .DeclareVar<object>("min", _min.MakeCodegen(classScope, method))
                .DeclareVar<object>("max", _max.MakeCodegen(classScope, method))
                .DeclareVar<object>(
                    "value",
                    NewInstance(returnType, Cast(castType, Ref("min")), Cast(castType, Ref("max"))))
                .BlockReturn(FilterValueSetParamImpl.CodegenNew(Ref("value")));

            method.Block.MethodReturn(param);
            return LocalMethod(method);
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            @out.Append("lower ");
            _min.ValueToString(@out);
            @out.Append(" upper ");
            _max.ValueToString(@out);
        }
    }
} // end of namespace