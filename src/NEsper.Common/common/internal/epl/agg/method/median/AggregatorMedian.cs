///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.median
{
    public class AggregatorMedian : AggregatorMethodWDistinctWFilterWValueBase
    {
        private CodegenExpressionMember _vector;

        public AggregatorMedian(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter) : base(optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
        }

        public override void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            _vector = membersColumnized.AddMember(col, typeof(SortedDoubleVector), "vector");
            rowCtor.Block.AssignRef(_vector, NewInstance(typeof(SortedDoubleVector)));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                _vector,
                "Add",
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(value, valueType));
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                _vector,
                "Remove",
                SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(value, valueType));
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                _vector,
                "Add",
                ExprDotMethod(Cast(typeof(object), value), "AsDouble"));
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(
                _vector,
                "Remove",
                ExprDotMethod(Cast(typeof(object), value), "AsDouble"));
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(_vector, "Clear");
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.MethodReturn(StaticMethod(typeof(AggregatorMedian), "MedianCompute", _vector));
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
            method.Block
                .StaticMethod(typeof(SortedDoubleVector), "WritePoints", output, RowDotMember(row, _vector));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row,
            int col,
            CodegenExpressionRef input,
            CodegenExpressionRef unitKey,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .AssignRef(RowDotMember(row, _vector), StaticMethod(typeof(SortedDoubleVector), "ReadPoints", input));
        }

        protected override void AppendFormatWODistinct(FabricTypeCollector collector)
        {
            collector.SortedDoubleVector();
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="vector">vector</param>
        /// <returns>value</returns>
        public static object MedianCompute(SortedDoubleVector vector)
        {
            if (vector.Count == 0) {
                return null;
            }

            if (vector.Count == 1) {
                return vector[0];
            }

            var middle = vector.Count >> 1;
            if (vector.Count % 2 == 0) {
                return (vector[middle - 1] + vector[middle]) / 2;
            }

            return vector[middle];
        }
    }
} // end of namespace