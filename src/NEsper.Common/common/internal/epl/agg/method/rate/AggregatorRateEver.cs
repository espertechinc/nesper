///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.rate
{
    /// <summary>
    /// Aggregation computing an event arrival rate for with and without data window.
    /// </summary>
    public class AggregatorRateEver : AggregatorMethodWDistinctWFilterBase
    {
        private readonly AggregationForgeFactoryRate _factory;
        private CodegenExpressionMember _points;
        private CodegenExpressionMember _hasLeave;

        public AggregatorRateEver(
            AggregationForgeFactoryRate factory,
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter) : base(optionalDistinctValueType, optionalDistinctSerde, hasFilter, optionalFilter)
        {
            _factory = factory;
        }

        public override void InitForgeFiltered(
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope)
        {
            _points = membersColumnized.AddMember(col, typeof(Deque<long>), "points");
            _hasLeave = membersColumnized.AddMember(col, typeof(bool), "hasLeave");
            rowCtor.Block.AssignRef(_points, NewInstance(typeof(ArrayDeque<long>)));
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
            method.Block.ExprDotMethod(_points, "Clear");
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(Not(ExprDotMethod(_points, "IsEmpty")))
                .DeclareVar<long>("newest", ExprDotMethod(ExprDotName(_points, "Last"), "AsInt64"))
                .DeclareVar<bool>(
                    "leave",
                    StaticMethod(
                        typeof(AggregatorRateEver),
                        "RemoveFromHead",
                        _points,
                        Ref("newest"),
                        Constant(_factory.IntervalTime)))
                .AssignCompound(_hasLeave, "|", Ref("leave"))
                .BlockEnd()
                .IfCondition(Not(_hasLeave))
                .BlockReturn(ConstantNull())
                .IfCondition(ExprDotMethod(_points, "IsEmpty"))
                .BlockReturn(Constant(0d))
                .MethodReturn(
                    Op(
                        Op(
                            Op(ExprDotName(_points, "Count"), "*", Constant(_factory.TimeAbacus.OneSecond)),
                            "*",
                            Constant(1d)),
                        "/",
                        Constant(_factory.IntervalTime)));
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
                .Apply(WriteBoolean(output, row, _hasLeave))
                .StaticMethod(typeof(AggregatorRateEverSerde), "WritePoints", output, RowDotMember(row, _points));
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
                .Apply(ReadBoolean(row, _hasLeave, input))
                .AssignRef(
                    RowDotMember(row, _points),
                    StaticMethod(typeof(AggregatorRateEverSerde), "ReadPoints", input));
        }

        protected override void AppendFormatWODistinct(FabricTypeCollector collector)
        {
            collector.Builtin(typeof(bool));
            collector.AggregatorRateEver(AggregatorRateEverSerde.SERDE_VERSION);
        }

        protected void Apply(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            CodegenExpression timeProvider = classScope.AddOrGetDefaultFieldSharable(TimeProviderField.INSTANCE);
            method.Block
                .DeclareVar<long>("timestamp", ExprDotName(timeProvider, "Time"))
                .ExprDotMethod(_points, "Add", Ref("timestamp"))
                .DeclareVar<bool>(
                    "leave",
                    StaticMethod(
                        typeof(AggregatorRateEver),
                        "RemoveFromHead",
                        _points,
                        Ref("timestamp"),
                        Constant(_factory.IntervalTime)))
                .AssignCompound(_hasLeave, "|", Ref("leave"));
        }
        
        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
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
                    var first = points.First;
                    var delta = timestamp - first;
                    if (delta >= interval) {
                        points.RemoveFirst();
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