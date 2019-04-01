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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    public class AggregationPortableValidationMinMax : AggregationPortableValidationWFilterWInputType
    {
        public AggregationPortableValidationMinMax(
            bool distinct, bool hasFilter, Type inputValueType, MinMaxTypeEnum minMax, bool unbound)
            : base(distinct, hasFilter, inputValueType)

        {
            MinMax = minMax;
            Unbound = unbound;
        }

        public AggregationPortableValidationMinMax()
        {
        }

        public bool Unbound { get; set; }

        public MinMaxTypeEnum MinMax { get; set; }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationMinMax);
        }

        protected override void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref, CodegenMethod method, ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(@ref, "setUnbound", Constant(Unbound))
                .ExprDotMethod(@ref, "setMinMax", Constant(MinMax));
        }

        protected override void ValidateIntoTableWFilterWInputType(
            string tableExpression, AggregationPortableValidation intoTableAgg, string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationMinMax) intoTableAgg;
            if (MinMax != that.MinMax) {
                throw new ExprValidationException(
                    "The aggregation declares " +
                    MinMax.ExpressionText +
                    " and provided is " +
                    that.MinMax.ExpressionText);
            }

            AggregationValidationUtil.ValidateAggregationUnbound(Unbound, that.Unbound);
        }
    }
} // end of namespace