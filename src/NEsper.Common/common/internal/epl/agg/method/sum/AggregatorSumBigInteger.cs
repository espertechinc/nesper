///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.common.@internal.serde.serdeset.builtin;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    /// <summary>
    ///     Sum for BigInteger values.
    /// </summary>
    public class AggregatorSumBigInteger : AggregatorSumBase
    {
        public AggregatorSumBigInteger(
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
            if (!sumType.IsTypeBigInteger()) {
                throw new ArgumentException("Invalid type " + sumType);
            }
        }

        protected override CodegenExpression InitOfSum()
        {
            return EnumValue(typeof(BigInteger), "Zero");
        }

        protected override void ApplyAggEnterSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            method.Block.AssignRef(
                sum,
                StaticMethod(
                    typeof(BigInteger),
                    "Add",
                    sum,
                    valueType == sumType ? value : FlexCast(sumType, value)));
        }

        protected override void ApplyAggLeaveSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            method.Block.AssignRef(
                sum,
                StaticMethod(
                    typeof(BigInteger),
                    "Subtract",
                    sum,
                    valueType == sumType ? value : FlexCast(sumType, value)));
        }

        protected override void ApplyTableEnterSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            //var valueType = method.LocalParams.First(p => p.Name == value.Ref).Type;
            method.Block.AssignRef(
                sum,
                StaticMethod(
                    typeof(BigInteger),
                    "Add",
                    sum,
                    FlexCast(evaluationTypes[0], value)));
            // ExprDotMethod(sum, "Add", FlexCast(evaluationTypes[0], value)));
        }

        protected override void ApplyTableLeaveSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            //var valueType = method.LocalParams.First(p => p.Name == value.Ref).Type;
            method.Block.AssignRef(
                sum,
                StaticMethod(
                    typeof(BigInteger),
                    "Subtract",
                    sum,
                    FlexCast(evaluationTypes[0], value)));
        }

        protected override void WriteSum(
            CodegenExpressionRef row,
            CodegenExpressionRef output,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType.IsTypeBigInteger()) {
                method.Block.StaticMethod(
                    typeof(DIOBigIntegerUtil),
                    "WriteBigInt",
                    RowDotMember(row, sum),
                    output);
            }
            else {
                throw new IllegalStateException("Codegen can only be performed on BigIntegers");
            }
        }

        protected override void ReadSum(
            CodegenExpressionRef row,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType.IsTypeBigInteger()) {
                method.Block.AssignRef(
                    RowDotMember(row, sum),
                    StaticMethod(typeof(DIOBigIntegerUtil), "ReadBigInt", input));
            }
            else {
                throw new IllegalStateException("Codegen can only be performed on BigIntegers");
            }
        }
        
        protected override void AppendSumFormat(FabricTypeCollector collector)
        {
            if (sumType == typeof(BigInteger)) {
                collector.BigInteger();
            }
            else {
                collector.BigDecimal();
            }
        }
    }
} // end of namespace