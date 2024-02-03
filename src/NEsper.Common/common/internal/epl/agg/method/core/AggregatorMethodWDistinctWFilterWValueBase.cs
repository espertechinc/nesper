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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.core
{
    public abstract class AggregatorMethodWDistinctWFilterWValueBase : AggregatorMethodWDistinctWFilterBase
    {
        protected abstract void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope);

        protected abstract void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope);

        protected abstract void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope);

        protected abstract void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope);

        public AggregatorMethodWDistinctWFilterWValueBase(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter) : base(optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
        }

        protected void ApplyEvalUnbox(
            CodegenMethod method,
            ExprForge[] forges,
            out CodegenExpressionRef value,
            out Type valueType)
        {
            // it is possible that "val" is actually a "nullable" type even though we
            // have checked the value.  In this block, we unbox the type and unbox the
            // value.
            value = Ref("val");
            valueType = forges[0].EvaluationType;
            if (valueType.CanBeNull()) {
                var valueTypeUnboxed = valueType.GetUnboxedType();
                if (valueTypeUnboxed != valueType) {
                    method.Block.DeclareVar(valueTypeUnboxed, "uval", Unbox(value, valueType));
                    valueType = valueTypeUnboxed;
                    value = Ref("uval");
                }
            }
        }
        
        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalValuePrefix(true, method, symbols, forges, classScope);
            ApplyEvalUnbox(method, forges, out var value, out var valueType);
            ApplyEvalEnterNonNull(value, valueType, method, symbols, forges, classScope);
        }

        protected override void ApplyTableEnterFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyTableValuePrefix(true, value, method, classScope);
            ApplyTableEnterNonNull(value, evaluationTypes, method, classScope);
        }

        protected override void ApplyEvalLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            ApplyEvalValuePrefix(false, method, symbols, forges, classScope);
            ApplyEvalUnbox(method, forges, out var value, out var valueType);
            ApplyEvalLeaveNonNull(value, valueType, method, symbols, forges, classScope);
        }

        protected override void ApplyTableLeaveFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyTableValuePrefix(false, value, method, classScope);
            ApplyTableLeaveNonNull(value, evaluationTypes, method, classScope);
        }

        private void ApplyEvalValuePrefix(
            bool enter,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            var type = forges[0].EvaluationType;
            var expr = forges[0].EvaluateCodegen(type, method, symbols, classScope);
            method.Block.DeclareVar(type, "val", expr);
            if (type.CanBeNull()) {
                method.Block.IfRefNull("val").BlockReturnNoValue();
            }

            if (Distinct != null) {
                method.Block
                    .IfCondition(Not(ExprDotMethod(Distinct, enter ? "Add" : "Remove", ToDistinctValueKey(Ref("val")))))
                    .BlockReturnNoValue();
            }
        }

        private void ApplyTableValuePrefix(
            bool enter,
            CodegenExpressionRef value,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(EqualsNull(value)).BlockReturnNoValue();
            if (Distinct != null) {
                method.Block
                    .IfCondition(Not(ExprDotMethod(Distinct, enter ? "Add" : "Remove", ToDistinctValueKey(value))))
                    .BlockReturnNoValue();
            }
        }
    }
} // end of namespace