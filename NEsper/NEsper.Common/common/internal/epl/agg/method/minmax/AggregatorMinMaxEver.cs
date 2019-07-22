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
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.core.MinMaxTypeEnum;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    /// <summary>
    ///     Min/max aggregator for all values, not considering events leaving the aggregation (i.e. ever).
    /// </summary>
    public class AggregatorMinMaxEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef currentMinMax;

        private readonly AggregationFactoryMethodMinMax factory;
        private readonly CodegenExpressionField serde;

        public AggregatorMinMaxEver(
            AggregationFactoryMethodMinMax factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            bool hasFilter,
            ExprNode optionalFilter)
            : base(
                factory,
                col,
                rowCtor,
                membersColumnized,
                classScope,
                optionalDistinctValueType,
                hasFilter,
                optionalFilter)
        {
            this.factory = factory;
            currentMinMax = membersColumnized.AddMember(col, typeof(IComparable), "currentMinMax");
            serde = classScope.AddOrGetFieldSharable(new CodegenSharableSerdeClassTyped(VALUE_NULLABLE, factory.type));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.Apply(EnterConsumer(value));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Apply(EnterConsumer(Cast(typeof(IComparable), value)));
        }

        public override void ApplyEvalLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            // no-op, this is designed to handle min-max ever
        }

        public override void ApplyTableLeaveCodegen(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // no-op
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            // no-op, this is designed to handle min-max ever
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // no-op
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(currentMinMax, ConstantNull());
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(currentMinMax);
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef output,
            CodegenExpressionRef unitKey,
            CodegenExpressionRef writer,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.Expression(
                WriteNullable(RowDotRef(row, currentMinMax), serde, output, unitKey, writer, classScope));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                RowDotRef(row, currentMinMax),
                Cast(typeof(IComparable), ReadNullable(serde, input, unitKey, classScope)));
        }

        private Consumer<CodegenBlock> EnterConsumer(CodegenExpression valueComparableTyped)
        {
            return block => block.IfCondition(EqualsNull(currentMinMax))
                .AssignRef(currentMinMax, valueComparableTyped)
                .BlockReturnNoValue()
                .IfCondition(
                    Relational(
                        ExprDotMethod(currentMinMax, "compareTo", valueComparableTyped),
                        factory.Parent.MinMaxTypeEnum == MAX ? LT : GT,
                        Constant(0)))
                .AssignRef(currentMinMax, valueComparableTyped);
        }
    }
} // end of namespace