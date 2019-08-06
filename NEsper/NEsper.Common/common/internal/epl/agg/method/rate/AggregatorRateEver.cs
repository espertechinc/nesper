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
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
    /// <summary>
    ///     Aggregation computing an event arrival rate for with and without data window.
    /// </summary>
    public class AggregatorRateEver : AggregatorMethodWDistinctWFilterBase
    {
        internal readonly AggregationFactoryMethodRate factory;
        internal readonly CodegenExpressionRef hasLeave;
        internal readonly CodegenExpressionRef points;

        public AggregatorRateEver(
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
            points = membersColumnized.AddMember(col, typeof(Deque<long>), "points");
            hasLeave = membersColumnized.AddMember(col, typeof(bool), "hasLeave");
            rowCtor.Block.AssignRef(points, NewInstance(typeof(ArrayDeque<long>)));
        }

        protected override void ApplyEvalEnterFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            Apply(method, classScope);
        }

        public override void ApplyEvalLeaveCodegen(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
            // This is an "ever" aggregator and is designed for use in non-window env
        }

        protected override void ApplyEvalLeaveFiltered(
            CodegenMethod method,
            ExprForgeCodegenSymbol symbols,
            ExprForge[] forges,
            CodegenClassScope classScope)
        {
        }

        protected override void ApplyTableEnterFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            Apply(method, classScope);
        }

        protected override void ApplyTableLeaveFiltered(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            // This is an "ever" aggregator and is designed for use in non-window env
        }

        protected override void ClearWODistinct(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.ExprDotMethod(points, "Clear");
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.IfCondition(Not(ExprDotMethod(points, "IsEmpty")))
                .DeclareVar<long>("newest", Cast(typeof(long), ExprDotName(points, "Last")))
                .DeclareVar<bool>(
                    "leave",
                    StaticMethod(
                        typeof(AggregatorRateEver),
                        "RemoveFromHead",
                        points,
                        Ref("newest"),
                        Constant(factory.IntervalTime)))
                .AssignCompound(hasLeave, "|", Ref("leave"))
                .BlockEnd()
                .IfCondition(Not(hasLeave))
                .BlockReturn(ConstantNull())
                .IfCondition(ExprDotMethod(points, "IsEmpty"))
                .BlockReturn(Constant(0d))
                .MethodReturn(
                    Op(
                        Op(
                            Op(ExprDotMethod(points, "Size"), "*", Constant(factory.TimeAbacus.OneSecond)),
                            "*",
                            Constant(1d)),
                        "/",
                        Constant(factory.IntervalTime)));
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
                .Apply(WriteBoolean(output, row, hasLeave))
                .StaticMethod(GetType(), "WritePoints", output, RowDotRef(row, points));
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
                .Apply(ReadBoolean(row, hasLeave, input))
                .AssignRef(RowDotRef(row, points), StaticMethod(GetType(), "ReadPoints", input));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">out</param>
        /// <param name="points">points</param>
        /// <throws>IOException io error</throws>
        public static void WritePoints(
            DataOutput output,
            Deque<long> points)
        {
            output.WriteInt(points.Count);
            foreach (long value in points) {
                output.WriteLong(value);
            }
        }

        protected void Apply(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            CodegenExpression timeProvider = classScope.AddOrGetFieldSharable(TimeProviderField.INSTANCE);
            method.Block.DeclareVar<long>("timestamp", ExprDotName(timeProvider, "Time"))
                .ExprDotMethod(points, "Add", Ref("timestamp"))
                .DeclareVar<bool>(
                    "leave",
                    StaticMethod(
                        typeof(AggregatorRateEver),
                        "RemoveFromHead",
                        points,
                        Ref("timestamp"),
                        Constant(factory.IntervalTime)))
                .AssignCompound(hasLeave, "|", Ref("leave"));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>points</returns>
        /// <throws>IOException io error</throws>
        public static Deque<long> ReadPoints(DataInput input)
        {
            var points = new ArrayDeque<long>();
            var size = input.ReadInt();
            for (var i = 0; i < size; i++) {
                points.Add(input.ReadLong());
            }

            return points;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="points">points</param>
        /// <param name="timestamp">timestamp</param>
        /// <param name="interval">interval</param>
        /// <returns>hasLeave</returns>
        public static bool RemoveFromHead(
            Deque<long> points,
            long timestamp,
            long interval)
        {
            var hasLeave = false;
            if (points.Count > 1) {
                while (true) {
                    long first = points.First;
                    var delta = timestamp - first;
                    if (delta >= interval) {
                        points.RemoveFirst();
                        //points.Remove();
                        hasLeave = true;
                    }
                    else {
                        break;
                    }

                    if (points.IsEmpty()) {
                        break;
                    }
                }
            }

            return hasLeave;
        }
    }
} // end of namespace