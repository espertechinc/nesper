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
        protected CodegenExpressionRef accumulator;

        protected AggregationFactoryMethodRate factory;
        protected CodegenExpressionRef isSet;
        protected CodegenExpressionRef latest;
        protected CodegenExpressionRef oldest;

        public AggregatorRate(
            AggregationFactoryMethodRate factory,
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
            accumulator = membersColumnized.AddMember(col, typeof(double), "accumulator");
            latest = membersColumnized.AddMember(col, typeof(long), "latest");
            oldest = membersColumnized.AddMember(col, typeof(long), "oldest");
            isSet = membersColumnized.AddMember(col, typeof(bool), "isSet");
        }

        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            var firstType = forges[0].EvaluationType;
            var firstExpr = forges[0].EvaluateCodegen(typeof(long), method, symbols, classScope);
            method.Block.AssignRef(latest, SimpleNumberCoercerFactory.CoercerLong.CodegenLong(firstExpr, firstType));

            var numFilters = factory.Parent.OptionalFilter != null ? 1 : 0;
            if (forges.Length == numFilters + 1) {
                method.Block.Increment(accumulator);
            }
            else {
                var secondType = forges[1].EvaluationType;
                var secondExpr = forges[1].EvaluateCodegen(typeof(double), method, symbols, classScope);
                var secondValue = SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(
                    secondExpr, secondType);
                method.Block.AssignCompound(
                    accumulator,
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
            var numFilters = factory.Parent.OptionalFilter != null ? 1 : 0;

            var firstType = forges[0].EvaluationType;
            var firstExpr = forges[0].EvaluateCodegen(typeof(long), method, symbols, classScope);
            var firstValue = SimpleNumberCoercerFactory.CoercerLong.CodegenLong(firstExpr, firstType);

            method.Block
                .AssignRef(oldest, firstValue)
                .IfCondition(Not(isSet))
                .AssignRef(isSet, ConstantTrue());
            if (forges.Length == numFilters + 1) {
                method.Block.Decrement(accumulator);
            }
            else {
                var secondType = forges[1].EvaluationType;
                var secondExpr = forges[1].EvaluateCodegen(typeof(double), method, symbols, classScope);
                var secondValue = SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(secondExpr, secondType);
                method.Block.AssignCompound(accumulator, "-", secondValue);
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
            method.Block.AssignRef(accumulator, Constant(0))
                .AssignRef(latest, Constant(0))
                .AssignRef(oldest, Constant(0));
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Not(isSet))
                .BlockReturn(ConstantNull())
                .MethodReturn(
                    Op(
                        Op(accumulator, "*", Constant(factory.TimeAbacus.OneSecond)),
                        "/",
                        Op(latest, "-", oldest)));
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
                .Apply(WriteDouble(output, row, accumulator))
                .Apply(WriteLong(output, row, latest))
                .Apply(WriteLong(output, row, oldest))
                .Apply(WriteBoolean(output, row, isSet));
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
                .Apply(ReadDouble(row, accumulator, input))
                .Apply(ReadLong(row, latest, input))
                .Apply(ReadLong(row, oldest, input))
                .Apply(ReadBoolean(row, isSet, input));
        }
    }
} // end of namespace