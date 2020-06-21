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
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.CodegenRelational;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.expression.core.MinMaxTypeEnum;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    /// <summary>
    ///     Min/max aggregator for all values, not considering events leaving the aggregation (i.e. ever).
    /// </summary>
    public class AggregatorMinMaxEver : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly AggregationForgeFactoryMinMax _factory;
        private readonly CodegenExpressionMember _currentMinMax;
        private readonly CodegenExpressionInstanceField _serde;

        public AggregatorMinMaxEver(
            AggregationForgeFactoryMinMax factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            DataInputOutputSerdeForge serde)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            _factory = factory;
            _currentMinMax = membersColumnized.AddMember(col, typeof(IComparable), "currentMinMax");
            _serde = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(
                    CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.VALUE_NULLABLE,
                    factory.ResultType,
                    serde,
                    classScope));
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
            method.Block.AssignRef(_currentMinMax, ConstantNull());
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(_currentMinMax);
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
                WriteNullable(RowDotMember(row, _currentMinMax), _serde, output, unitKey, writer, classScope));
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
                RowDotMember(row, _currentMinMax),
                Cast(typeof(IComparable), ReadNullable(_serde, input, unitKey, classScope)));
        }

        private Consumer<CodegenBlock> EnterConsumer(CodegenExpression valueComparableTyped)
        {
            return block => block.IfCondition(EqualsNull(_currentMinMax))
                .AssignRef(_currentMinMax, valueComparableTyped)
                .BlockReturnNoValue()
                .IfCondition(
                    Relational(
                        ExprDotMethod(_currentMinMax, "CompareTo", valueComparableTyped),
                        _factory.Parent.MinMaxTypeEnum == MAX ? LT : GT,
                        Constant(0)))
                .AssignRef(_currentMinMax, valueComparableTyped);
        }
    }
} // end of namespace