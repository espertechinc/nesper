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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    public class AggregatorSumNumeric : AggregatorSumBase
    {
        public AggregatorSumNumeric(
            Type optionalDistinctValueType,
            DataInputOutputSerdeForge optionalDistinctSerde,
            bool hasFilter,
            ExprNode optionalFilter,
            Type sumType)
            : base(
                optionalDistinctValueType,
                optionalDistinctSerde,
                hasFilter,
                optionalFilter,
                sumType)
        {
            if (!sumType.IsTypeInt32() &&
                !sumType.IsTypeInt64() &&
                !sumType.IsTypeDecimal() &&
                !sumType.IsTypeDouble() &&
                !sumType.IsTypeSingle()) {
                throw new ArgumentException("Invalid sum type " + sumType);
            }
        }

        protected override CodegenExpression InitOfSum()
        {
            if (sumType == typeof(decimal?)) {
                return Constant(0.0m);
            }
            else if (sumType == typeof(double?)) {
                return Constant(0.0d);
            }
            else if (sumType == typeof(float?)) {
                return Constant(0.0f);
            }
            else if (sumType == typeof(long?)) {
                return Constant(0L);
            }

            return Constant(0);
        }

        protected override void ApplyAggEnterSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            ApplyAgg(true, value, valueType, method);
        }

        protected override void ApplyTableEnterSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyTable(true, value, method, classScope);
        }

        protected override void ApplyAggLeaveSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            ApplyAgg(false, value, valueType, method);
        }

        protected override void ApplyTableLeaveSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            ApplyTable(false, value, method, classScope);
        }

        protected override void WriteSum(
            CodegenExpressionRef row,
            CodegenExpressionRef output,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType == typeof(decimal?)) {
                method.Block.Apply(WriteDecimal(output, row, sum));
            }
            else if (sumType == typeof(double?)) {
                method.Block.Apply(WriteDouble(output, row, sum));
            }
            else if (sumType == typeof(float?)) {
                method.Block.Apply(WriteFloat(output, row, sum));
            }
            else if (sumType == typeof(long?)) {
                method.Block.Apply(WriteLong(output, row, sum));
            }
            else if (sumType == typeof(int?)) {
                method.Block.Apply(WriteInt(output, row, sum));
            }
            else {
                throw new IllegalStateException("Unrecognized sum type " + sumType);
            }
        }

        protected override void ReadSum(
            CodegenExpressionRef row,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType == typeof(decimal?)) {
                method.Block.Apply(ReadDecimal(row, sum, input));
            }
            else if (sumType == typeof(double?)) {
                method.Block.Apply(ReadDouble(row, sum, input));
            }
            else if (sumType == typeof(float?)) {
                method.Block.Apply(ReadFloat(row, sum, input));
            }
            else if (sumType == typeof(long?)) {
                method.Block.Apply(ReadLong(row, sum, input));
            }
            else if (sumType == typeof(int?)) {
                method.Block.Apply(ReadInt(row, sum, input));
            }
            else {
                throw new IllegalStateException("Unrecognized sum type " + sumType);
            }
        }
        
        protected override void AppendSumFormat(FabricTypeCollector collector)
        {
            collector.Builtin(sumType.GetPrimitiveType());
        }

        private void ApplyAgg(
            bool enter,
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            var coercer = AggregationForgeFactorySum.GetCoercerNonBigInt(valueType);
            var opcode = enter ? "+" : "-";
            method.Block.AssignRef(sum, Op(sum, opcode, coercer.CoerceCodegen(value, valueType)));
        }

        private void ApplyTable(
            bool enter,
            CodegenExpressionRef value,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            var coercer = AggregationForgeFactorySum.GetCoercerNonBigInt(sumType);
            var opcode = enter ? "+" : "-";
            method.Block.AssignRef(sum, Op(sum, opcode, coercer.CoerceCodegen(value, typeof(object))));
        }
    }
} // end of namespace