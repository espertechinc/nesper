///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.firstlastever
{
    /// <summary>
    ///     Aggregator for the very last value.
    /// </summary>
    public class AggregatorLastEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef lastValue;
        private readonly CodegenExpressionField serde;

        public AggregatorLastEver(
            AggregationForgeFactory factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter,
            Type childType)
            : base(
                factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter,
                optionalFilter)
        {
            lastValue = membersColumnized.AddMember(col, typeof(object), "lastValue");
            serde = classScope.AddOrGetFieldSharable(new CodegenSharableSerdeClassTyped(VALUE_NULLABLE, childType));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block.AssignRef(lastValue, forges[0].EvaluateCodegen(typeof(object), method, symbols, classScope));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(lastValue, value);
        }

        public override void ApplyEvalLeaveCodegen(
            CodegenMethod method, ExprForgeCodegenSymbol symbols, ExprForge[] forges, CodegenClassScope classScope)
        {
        }

        public override void ApplyTableLeaveCodegen(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            base.ApplyTableLeaveCodegen(value, evaluationTypes, method, classScope);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
        }

        protected override void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(lastValue, ConstantNull());
        }

        public override void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(lastValue);
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.Expression(
                WriteNullable(RowDotRef(row, lastValue), serde, output, unitKey, writer, classScope));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(RowDotRef(row, lastValue), ReadNullable(serde, input, unitKey, classScope));
        }
    }
} // end of namespace