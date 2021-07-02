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

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
    public class AggregationPortableValidationRate : AggregationPortableValidationWFilterWInputType
    {
        public AggregationPortableValidationRate(
            bool distinct,
            bool hasFilter,
            Type inputValueType,
            long intervalTime)
            : base(distinct, hasFilter, inputValueType)
        {
            IntervalTime = intervalTime;
        }

        public AggregationPortableValidationRate()
        {
        }

        public long IntervalTime { get; set; }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationRate);
        }

        protected override void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(@ref, "IntervalTime", Constant(IntervalTime));
        }

        protected override void ValidateIntoTableWFilterWInputType(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationRate) intoTableAgg;
            if (IntervalTime != that.IntervalTime) {
                throw new ExprValidationException(
                    "The interval-time is " +
                    IntervalTime +
                    " and provided is " +
                    that.IntervalTime);
            }
        }
    }
} // end of namespace