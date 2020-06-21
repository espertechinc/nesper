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

namespace com.espertech.esper.common.@internal.epl.agg.method.nth
{
    public class AggregationPortableValidationNth : AggregationPortableValidationWFilterWInputType
    {
        public AggregationPortableValidationNth(
            bool distinct,
            bool hasFilter,
            Type inputValueType,
            int size)
            : base(distinct, hasFilter, inputValueType)

        {
            this.Size = size;
        }

        public AggregationPortableValidationNth()
        {
        }

        public int Size { get; set; }

        protected override Type TypeOf()
        {
            return typeof(AggregationPortableValidationNth);
        }

        protected override void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block.SetProperty(@ref, "Size", Constant(Size));
        }

        protected override void ValidateIntoTableWFilterWInputType(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationPortableValidationNth that = (AggregationPortableValidationNth) intoTableAgg;
            if (Size != that.Size) {
                throw new ExprValidationException(
                    "The size is " +
                    Size +
                    " and provided is " +
                    that.Size);
            }
        }
    }
} // end of namespace