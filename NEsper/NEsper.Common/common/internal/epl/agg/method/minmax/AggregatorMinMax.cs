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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.compiletime.sharable;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    /// <summary>
    ///     Min/max aggregator for all values.
    /// </summary>
    public class AggregatorMinMax : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly AggregationForgeFactoryMinMax _factory;
        private readonly CodegenExpressionMember _refSet;
        private readonly CodegenExpressionInstanceField _serde;

        public AggregatorMinMax(
                AggregationForgeFactoryMinMax factory,
                int col,
                CodegenCtor rowCtor,
                CodegenMemberCol membersColumnized,
                CodegenClassScope classScope,
                Type optionalDistinctValueType,
                DataInputOutputSerdeForge optionalDistinctSerde,
                bool hasFilter,
                ExprNode optionalFilter)
            : base(factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            this._factory = factory;
            _refSet = membersColumnized.AddMember(col, typeof(SortedRefCountedSet<object>), "refSet");
            _serde = classScope.AddOrGetDefaultFieldSharable(
                new CodegenSharableSerdeClassTyped(
                    CodegenSharableSerdeClassTyped.CodegenSharableSerdeName.SORTEDREFCOUNTEDSET,
                    factory.ResultType,
                    factory.Serde,
                    classScope));
            rowCtor.Block.AssignRef(_refSet, NewInstance(typeof(SortedRefCountedSet<object>)));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Add", value);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Add", value);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Remove", value);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Remove", value);
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_refSet, "Clear");
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(
                ExprDotName(_refSet, _factory.Parent.MinMaxTypeEnum == MinMaxTypeEnum.MAX ? "MaxValue" : "MinValue"));
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
            method.Block.ExprDotMethod(_serde, "Write", RowDotMember(row, _refSet), output, unitKey, writer);
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
                RowDotMember(row, _refSet),
                Cast(typeof(SortedRefCountedSet<object>), ExprDotMethod(_serde, "Read", input, unitKey)));
        }
    }
} // end of namespace