///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    public class AggregationPortableValidationMinMax : AggregationPortableValidationWFilterWInputType
    {
        public AggregationPortableValidationMinMax(
            bool distinct,
            bool hasFilter,
            Type inputValueType,
            MinMaxTypeEnum minMax,
            bool isUnbound)
            : base(distinct, hasFilter, inputValueType)

        {
            MinMax = minMax;
            IsUnbound = isUnbound;
        }

        public AggregationPortableValidationMinMax()
        {
        }

        public bool IsUnbound { get; set; }

        public MinMaxTypeEnum MinMax { get; set; }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationMinMax);
        }

        protected override void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(@ref, "IsUnbound", Constant(IsUnbound))
                .SetProperty(@ref, "MinMax", Constant(MinMax));
        }

        protected override void ValidateIntoTableWFilterWInputType(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationMinMax)intoTableAgg;
            if (MinMax != that.MinMax) {
                throw new ExprValidationException(
                    "The aggregation declares " +
                    MinMax.GetExpressionText() +
                    " and provided is " +
                    that.MinMax.GetExpressionText());
            }

            AggregationValidationUtil.ValidateAggregationUnbound(IsUnbound, that.IsUnbound);
        }
    }
} // end of namespace