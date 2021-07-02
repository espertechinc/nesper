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

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    public class AggregationPortableValidationFirstLastEver : AggregationPortableValidationWFilterWInputType
    {
        public AggregationPortableValidationFirstLastEver()
        {
        }

        public AggregationPortableValidationFirstLastEver(
            bool distinct,
            bool hasFilter,
            Type inputValueType,
            bool isFirst)
            : base(distinct, hasFilter, inputValueType)

        {
            IsFirst = isFirst;
        }

        public bool IsFirst { get; set; }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationFirstLastEver);
        }

        protected override void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(@ref, "IsFirst", Constant(IsFirst));
        }

        protected override void ValidateIntoTableWFilterWInputType(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationFirstLastEver) intoTableAgg;
            if (IsFirst != that.IsFirst) {
                throw new ExprValidationException(
                    "The aggregation declares " +
                    (IsFirst ? "firstever" : "lastever") +
                    " and provided is " +
                    (that.IsFirst ? "firstever" : "lastever"));
            }
        }
    }
} // end of namespace