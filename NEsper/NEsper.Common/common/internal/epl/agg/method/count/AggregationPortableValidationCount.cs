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

namespace com.espertech.esper.common.@internal.epl.agg.method.count
{
    public class AggregationPortableValidationCount : AggregationPortableValidationBase
    {
        private Type countedValueType;
        private bool ever;
        private bool hasFilter;
        private bool ignoreNulls;

        public AggregationPortableValidationCount()
        {
        }

        public AggregationPortableValidationCount(
            bool distinct,
            bool ever,
            bool hasFilter,
            Type countedValueType,
            bool ignoreNulls)
            : base(distinct)

        {
            this.ever = ever;
            this.hasFilter = hasFilter;
            this.countedValueType = countedValueType;
            this.ignoreNulls = ignoreNulls;
        }

        public bool IsEver {
            set => ever = value;
        }

        public bool HasFilter {
            set => hasFilter = value;
        }

        public Type CountedValueType {
            set => countedValueType = value;
        }

        public bool IgnoreNulls {
            set => ignoreNulls = value;
        }

        protected override void ValidateIntoTable(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationCount) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationFilter(hasFilter, that.hasFilter);
            if (IsDistinct) {
                AggregationValidationUtil.ValidateAggregationInputType(countedValueType, that.countedValueType);
            }

            if (ignoreNulls != that.ignoreNulls) {
                throw new ExprValidationException(
                    "The aggregation declares" +
                    (ignoreNulls ? "" : " no") +
                    " ignore nulls and provided is" +
                    (that.ignoreNulls ? "" : " no") +
                    " ignore nulls");
            }
        }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationCount);
        }

        protected override void CodegenInlineSet(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .SetProperty(@ref, "IsEver", Constant(ever))
                .SetProperty(@ref, "HasFilter", Constant(hasFilter))
                .SetProperty(@ref, "CountedValueType", Constant(countedValueType))
                .SetProperty(@ref, "IgnoreNulls", Constant(ignoreNulls));
        }
    }
} // end of namespace