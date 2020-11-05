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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
    /// <summary>
    ///     Aggregation computing an event arrival rate for data windowed-events.
    /// </summary>
    public class AggregatorRate : AggregatorMethodWDistinctWFilterBase
    {
        private AggregationForgeFactoryRate _factory;
        private CodegenExpressionMember _accumulator;
        private CodegenExpressionMember _isSet;
        private CodegenExpressionMember _latest;
        private CodegenExpressionMember _oldest;

        public AggregatorRate(
            AggregationForgeFactoryRate factory,
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
            _factory = factory;
            _accumulator = membersColumnized.AddMember(col, typeof(double), "accumulator");
            _latest = membersColumnized.AddMember(col, typeof(long), "latest");
            _oldest = membersColumnized.AddMember(col, typeof(long), "oldest");
            _isSet = membersColumnized.AddMember(col, typeof(bool), "isSet");
        }

        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            var firstType = forges[0].EvaluationType;
            var firstExpr = forges[0].EvaluateCodegen(typeof(long), method, symbols, classScope);
            method.Block.AssignRef(_latest, SimpleNumberCoercerFactory.CoercerLong.CodegenLong(firstExpr, firstType));

            var numFilters = _factory.Parent.OptionalFilter != null ? 1 : 0;
            if (forges.Length == numFilters + 1) {
                method.Block.Increment(_accumulator);
            }
            else {
                var secondType = forges[1].EvaluationType;
                var secondExpr = forges[1].EvaluateCodegen(typeof(double), method, symbols, classScope);
                var secondValue = SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(
                    secondExpr, secondType);
                method.Block.AssignCompound(
                    _accumulator,
                    "+",
                    secondValue);
            }
        }

        protected override void ApplyEvalLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            var numFilters = _factory.Parent.OptionalFilter != null ? 1 : 0;

            var firstType = forges[0].EvaluationType;
            var firstExpr = forges[0].EvaluateCodegen(typeof(long), method, symbols, classScope);
            var firstValue = SimpleNumberCoercerFactory.CoercerLong.CodegenLong(firstExpr, firstType);

            method.Block
                .AssignRef(_oldest, firstValue)
                .IfCondition(Not(_isSet))
                .AssignRef(_isSet, ConstantTrue());
            if (forges.Length == numFilters + 1) {
                method.Block.Decrement(_accumulator);
            }
            else {
                var secondType = forges[1].EvaluationType;
                var secondExpr = forges[1].EvaluateCodegen(typeof(double), method, symbols, classScope);
                var secondValue = SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(secondExpr, secondType);
                method.Block.AssignCompound(_accumulator, "-", secondValue);
            }
        }

        protected override void ApplyTableEnterFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            throw new UnsupportedOperationException("Not available with tables");
        }

        protected override void ApplyTableLeaveFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            throw new UnsupportedOperationException("Not available with tables");
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(_accumulator, Constant(0))
                .AssignRef(_latest, Constant(0))
                .AssignRef(_oldest, Constant(0));
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Not(_isSet))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Op(
                        Op(_accumulator, "*", Constant(_factory.TimeAbacus.OneSecond)),
                        "/",
                        Op(_latest, "-", _oldest)));
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
                .Apply(WriteDouble(output, row, _accumulator))
                .Apply(WriteLong(output, row, _latest))
                .Apply(WriteLong(output, row, _oldest))
                .Apply(WriteBoolean(output, row, _isSet));
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
                .Apply(ReadDouble(row, _accumulator, input))
                .Apply(ReadLong(row, _latest, input))
                .Apply(ReadLong(row, _oldest, input))
                .Apply(ReadBoolean(row, _isSet, input));
        }
    }
} // end of namespace