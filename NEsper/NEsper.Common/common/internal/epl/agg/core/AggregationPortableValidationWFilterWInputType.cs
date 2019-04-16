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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public abstract class AggregationPortableValidationWFilterWInputType : AggregationPortableValidationBase
    {
        protected bool hasFilter;
        protected Type inputValueType;

        protected AggregationPortableValidationWFilterWInputType()
        {
        }

        protected AggregationPortableValidationWFilterWInputType(
            bool distinct,
            bool hasFilter,
            Type inputValueType)
            : base(distinct)
        {
            this.hasFilter = hasFilter;
            this.inputValueType = inputValueType;
        }

        protected abstract void CodegenInlineSetWFilterWInputType(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope);

        protected abstract void ValidateIntoTableWFilterWInputType(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory);

        protected override void CodegenInlineSet(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            method.Block
                .ExprDotMethod(Ref("v"), "setInputValueType", Constant(inputValueType))
                .ExprDotMethod(Ref("v"), "setHasFilter", Constant(hasFilter));
            CodegenInlineSetWFilterWInputType(@ref, method, symbols, classScope);
        }

        protected override void ValidateIntoTable(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            var that = (AggregationPortableValidationWFilterWInputType) intoTableAgg;
            AggregationValidationUtil.ValidateAggregationInputType(inputValueType, that.inputValueType);
            AggregationValidationUtil.ValidateAggregationFilter(hasFilter, that.hasFilter);
            ValidateIntoTableWFilterWInputType(tableExpression, intoTableAgg, intoExpression, factory);
        }

        public void SetHasFilter(bool hasFilter)
        {
            this.hasFilter = hasFilter;
        }

        public void SetInputValueType(Type inputValueType)
        {
            this.inputValueType = inputValueType;
        }
    }
} // end of namespace