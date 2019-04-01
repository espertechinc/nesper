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
using com.espertech.esper.common.@internal.serde;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.serde.CodegenSharableSerdeClassTyped.CodegenSharableSerdeName;

namespace com.espertech.esper.common.@internal.epl.agg.method.minmax
{
    /// <summary>
    ///     Min/max aggregator for all values.
    /// </summary>
    public class AggregatorMinMax : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly AggregationFactoryMethodMinMax factory;
        private readonly CodegenExpressionRef refSet;
        private readonly CodegenExpressionField serde;

        public AggregatorMinMax(
            AggregationFactoryMethodMinMax factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter)
            : base(
                factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter,
                optionalFilter)
        {
            this.factory = factory;
            refSet = membersColumnized.AddMember(col, typeof(SortedRefCountedSet<object>), "refSet");
            serde = classScope.AddOrGetFieldSharable(
                new CodegenSharableSerdeClassTyped(SORTEDREFCOUNTEDSET, factory.type));
            rowCtor.Block.AssignRef(refSet, NewInstance(typeof(SortedRefCountedSet<object>)));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "add", value);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "add", value);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "remove", value);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "remove", value);
        }

        protected override void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(refSet, "clear");
        }

        public override void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(
                ExprDotMethod(refSet, factory.Parent.MinMaxTypeEnum == MinMaxTypeEnum.MAX ? "maxValue" : "minValue"));
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(serde, "write", RowDotRef(row, refSet), output, unitKey, writer);
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(
                RowDotRef(row, refSet),
                Cast(typeof(SortedRefCountedSet<object>), ExprDotMethod(serde, "read", input, unitKey)));
        }
    }
} // end of namespace