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
    public abstract class AggregationPortableValidationBase : AggregationPortableValidation
    {
        protected AggregationPortableValidationBase()
        {
        }

        protected AggregationPortableValidationBase(bool distinct)
        {
            this.IsDistinct = distinct;
        }

        public bool IsDistinct { get; set; }

        protected abstract Type TypeOf();

        protected abstract void CodegenInlineSet(
            CodegenExpressionRef @ref,
            CodegenMethod method,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope);

        protected abstract void ValidateIntoTable(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory);

        public void ValidateIntoTableCompatible(
            string tableExpression,
            AggregationPortableValidation intoTableAgg,
            string intoExpression,
            AggregationForgeFactory factory)
        {
            AggregationValidationUtil.ValidateAggregationType(this, tableExpression, intoTableAgg, intoExpression);
            var that = (AggregationPortableValidationBase) intoTableAgg;
            AggregationValidationUtil.ValidateDistinct(IsDistinct, that.IsDistinct);
            ValidateIntoTable(tableExpression, intoTableAgg, intoExpression, factory);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOf(), GetType(), classScope);
            method.Block
                .DeclareVar(TypeOf(), "v", NewInstance(TypeOf()))
                .SetProperty(Ref("v"), "IsDistinct", Constant(IsDistinct));
            CodegenInlineSet(Ref("v"), method, symbols, classScope);
            method.Block.MethodReturn(Ref("v"));
            return LocalMethod(method);
        }

        public AggregationPortableValidationBase SetDistinct(bool distinct)
        {
            this.IsDistinct = distinct;
            return this;
        }
    }
} // end of namespace