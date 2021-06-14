///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public abstract class AggregationPortableValidationBase : AggregationPortableValidation
    {
        public const string INVALID_TABLE_AGG_RESET = "The table aggregation'reset' method is only available for the on-merge update action";
        public const string INVALID_TABLE_AGG_RESET_PARAMS = "The table aggregation 'reset' method does not allow parameters";
        
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

        public bool IsAggregationMethod(
            string name,
            ExprNode[] parameters,
            ExprValidationContext validationContext)
        {
            return false;
        }

        public AggregationMultiFunctionMethodDesc ValidateAggregationMethod(
            ExprValidationContext validationContext,
            string aggMethodName,
            ExprNode[] parameters)
        {
            if (String.Equals(aggMethodName, "reset", StringComparison.InvariantCultureIgnoreCase)) {
                if (!validationContext.IsAllowTableAggReset) {
                    throw new ExprValidationException(INVALID_TABLE_AGG_RESET);
                }

                if (parameters.Length != 0) {
                    throw new ExprValidationException(INVALID_TABLE_AGG_RESET_PARAMS);
                }

                AggregationMethodForge reader = new ProxyAggregationMethodForge(
                    () => typeof(void),
                    (
                        parent,
                        symbols,
                        classScope) => ConstantNull());
                return new AggregationMultiFunctionMethodDesc(reader, null, null, null);
            }

            throw new ExprValidationException("Aggregation-method not supported for this type of aggregation");
        }
    }
} // end of namespace