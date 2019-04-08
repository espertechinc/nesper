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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.core;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.io;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.avedev
{
    public class AggregatorAvedev : AggregatorMethodWDistinctWFilterWValueBase
    {
        private readonly CodegenExpressionRef sum;
        private readonly CodegenExpressionRef valueSet;

        public AggregatorAvedev(
            AggregationForgeFactory factory, int col, CodegenCtor rowCtor, CodegenMemberCol membersColumnized,
            CodegenClassScope classScope, Type optionalDistinctValueType, bool hasFilter, ExprNode optionalFilter)
            : base(
                factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter,
                optionalFilter)
        {
            valueSet = membersColumnized.AddMember(col, typeof(RefCountedSet<double>), "valueSet");
            sum = membersColumnized.AddMember(col, typeof(double), "sum");
            rowCtor.Block.AssignRef(valueSet, NewInstance(typeof(RefCountedSet<double>)));
        }

        protected override void ApplyEvalEnterNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            ApplyCodegen(true, value, valueType, method);
        }

        protected override void ApplyEvalLeaveNonNull(
            CodegenExpressionRef value, Type valueType, CodegenMethod method, ExprForgeCodegenSymbol symbols,
            ExprForge[] forges, CodegenClassScope classScope)
        {
            ApplyCodegen(false, value, valueType, method);
        }

        protected override void ApplyTableEnterNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            ApplyTableCodegen(true, value, method);
        }

        protected override void ApplyTableLeaveNonNull(
            CodegenExpressionRef value, Type[] evaluationTypes, CodegenMethod method, CodegenClassScope classScope)
        {
            ApplyTableCodegen(false, value, method);
        }

        protected override void ClearWODistinct(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.AssignRef(sum, Constant(0))
                .ExprDotMethod(valueSet, "clear");
        }

        public override void GetValueCodegen(CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block.MethodReturn(StaticMethod(typeof(AggregatorAvedev), "computeAvedev", valueSet, sum));
        }

        protected override void WriteWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef output, CodegenExpressionRef unitKey,
            CodegenExpressionRef writer, CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block
                .Apply(WriteDouble(output, row, sum))
                .StaticMethod(GetType(), "writePoints", output, RowDotRef(row, valueSet));
        }

        protected override void ReadWODistinct(
            CodegenExpressionRef row, int col, CodegenExpressionRef input, CodegenExpressionRef unitKey,
            CodegenMethod method, CodegenClassScope classScope)
        {
            method.Block
                .Apply(ReadDouble(row, sum, input))
                .AssignRef(RowDotRef(row, valueSet), StaticMethod(GetType(), "readPoints", input));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="output">output</param>
        /// <param name="valueSet">values</param>
        /// <throws>IOException io error</throws>
        public static void WritePoints(DataOutput output, RefCountedSet<double> valueSet)
        {
            var refSet = valueSet.RefSet;
            output.WriteInt(refSet.Count);
            output.WriteInt(valueSet.NumValues);
            foreach (var entry in valueSet.RefSet) {
                output.WriteDouble(entry.Key);
                output.WriteInt(entry.Value);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="input">input</param>
        /// <returns>values</returns>
        /// <throws>IOException io error</throws>
        public static RefCountedSet<double> ReadPoints(DataInput input)
        {
            var valueSet = new RefCountedSet<double>();
            var und = valueSet.RefSet;
            var size = input.ReadInt();
            valueSet.NumValues = input.ReadInt();
            for (var i = 0; i < size; i++) {
                var key = input.ReadDouble();
                var val = input.ReadInt();
                und.Put(key, val);
            }

            return valueSet;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="valueSet">values</param>
        /// <param name="sum">sum</param>
        /// <returns>value</returns>
        public static object ComputeAvedev(RefCountedSet<double> valueSet, double sum)
        {
            var datapoints = valueSet.Count;

            if (datapoints == 0) {
                return null;
            }

            double total = 0;
            var avg = sum / datapoints;

            foreach (var entry in valueSet) {
                total += entry.Value * Math.Abs(entry.Key - avg);
            }

            return total / datapoints;
        }

        private void ApplyCodegen(bool enter, CodegenExpression value, Type valueType, CodegenMethod method)
        {
            method.Block
                .DeclareVar(
                    typeof(double), "d",
                    SimpleNumberCoercerFactory.CoercerDouble.CodegenDouble(value, valueType))
                .ExprDotMethod(valueSet, enter ? "add" : "remove", Ref("d"))
                .AssignCompound(sum, enter ? "+" : "-", Ref("d"));
        }

        private void ApplyTableCodegen(bool enter, CodegenExpression value, CodegenMethod method)
        {
            method.Block
                .DeclareVar(typeof(double), "d", ExprDotMethod(Cast(typeof(double), value), "doubleValue"))
                .ExprDotMethod(valueSet, enter ? "add" : "remove", Ref("d"))
                .AssignCompound(sum, enter ? "+" : "-", Ref("d"));
        }
    }
} // end of namespace