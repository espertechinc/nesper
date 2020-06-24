///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
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
        public FilterSpecParamConstantForge(
            ExprFilterSpecLookupableForge lookupable,
            FilterOperator filterOperator,
            object filterConstant)
            : base(lookupable, filterOperator)
        {
            FilterConstant = filterConstant;

            if (filterOperator.IsRangeOperator()) {
                throw new ArgumentException(
                    "Illegal filter operator " +
                    filterOperator +
                    " supplied to " +
                    "constant filter parameter");
            }
        }

        /// <summary>
        ///     Returns the constant value.
        /// </summary>
        /// <returns>constant value</returns>
        public object FilterConstant { get; }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols)
        {
            var method = parent
                .MakeChild(typeof(FilterSpecParam), typeof(FilterSpecParamConstantForge), classScope);
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

            //var inner = NewAnonymousClass(
            //    method.Block,
            //    typeof(FilterSpecParam),
            //    Arrays.AsList<CodegenExpression>(Ref("lookupable"), Ref("op")));
            //var getFilterValue = CodegenMethod.MakeParentNode(typeof(object), GetType(), classScope)
            //    .AddParam(FilterSpecParam.GET_FILTER_VALUE_FP);
            //inner.AddMethod("GetFilterValue", getFilterValue);

            getFilterValue.Block.BlockReturn(FilterValueSetParamImpl.CodegenNew(Constant(FilterConstant)));

            method.Block.MethodReturn(inner);
            return method;
        }

        public override string ToString()
        {
            return base.ToString() + " filterConstant=" + FilterConstant;
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

            if (FilterConstant != null ? !FilterConstant.Equals(that.FilterConstant) : that.FilterConstant != null) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var result = base.GetHashCode();
            result = 31 * result + (FilterConstant != null ? FilterConstant.GetHashCode() : 0);
            return result;
        }

        public override void ValueExprToString(
            StringBuilder @out,
            int i)
        {
            ValueExprToString(@out, FilterConstant);
        }

        public static void ValueExprToString(
            StringBuilder @out,
            Object constant)
        {
            var constantType = constant?.GetType();
            var constantTypeName = constantType?.CleanName();
            
            @out.Append("constant ");
            CodegenExpressionUtil.RenderConstant(@out, constant, EmptyDictionary<string, object>.Instance);
            @out.Append(" type ").Append(constantTypeName);
        }

        public static String ValueExprToString(Object constant)
        {
            var builder = new StringBuilder();
            ValueExprToString(builder, constant);
            return builder.ToString();
        }
    }
} // end of namespace