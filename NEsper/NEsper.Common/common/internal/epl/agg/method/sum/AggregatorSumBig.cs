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
using com.espertech.esper.common.@internal.serde;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    /// <summary>
    ///     Sum for BigInteger values.
    /// </summary>
    public class AggregatorSumBig : AggregatorSumBase
    {
        public AggregatorSumBig(
            AggregationForgeFactory factory,
            int col,
            CodegenCtor rowCtor,
            CodegenMemberCol membersColumnized,
            CodegenClassScope classScope,
            Type optionalDistinctValueType,
            bool hasFilter,
            ExprNode optionalFilter,
            Type sumType)
            : base(
                factory, col, rowCtor, membersColumnized, classScope, optionalDistinctValueType, hasFilter, optionalFilter,
                sumType)
        {
            if (sumType != typeof(BigInteger) && sumType != typeof(decimal?)) {
                throw new ArgumentException("Invalid type " + sumType);
            }
        }

        protected override CodegenExpression InitOfSum()
        {
            return sumType == typeof(BigInteger)
                ? StaticMethod(typeof(BigInteger), "valueOf", Constant(0))
                : NewInstance(typeof(decimal?), Constant(0d));
        }

        protected override void ApplyAggEnterSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            method.Block.AssignRef(sum, ExprDotMethod(sum, "add", valueType == sumType ? value : Cast(sumType, value)));
        }

        protected override void ApplyAggLeaveSum(
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            method.Block.AssignRef(
                sum, ExprDotMethod(sum, "subtract", valueType == sumType ? value : Cast(sumType, value)));
        }

        protected override void ApplyTableEnterSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(sum, ExprDotMethod(sum, "add", Cast(evaluationTypes[0], value)));
        }

        protected override void ApplyTableLeaveSum(
            CodegenExpressionRef value,
            Type[] evaluationTypes,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(sum, ExprDotMethod(sum, "subtract", Cast(evaluationTypes[0], value)));
        }

        protected override void WriteSum(
            CodegenExpressionRef row,
            CodegenExpressionRef output,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType == typeof(BigInteger)) {
                method.Block.StaticMethod(
                    typeof(DIOSerdeBigInteger), "writeBigInt", RowDotRef(row, sum), output);
            }
            else {
                method.Block.StaticMethod(
                    typeof(DIOSerdeBigInteger), "writeBigDec", RowDotRef(row, sum), output);
            }
        }

        protected override void ReadSum(
            CodegenExpressionRef row,
            CodegenExpressionRef input,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (sumType == typeof(BigInteger)) {
                method.Block.AssignRef(
                    RowDotRef(row, sum), StaticMethod(typeof(DIOSerdeBigInteger), "readBigInt", input));
            }
            else {
                method.Block.AssignRef(
                    RowDotRef(row, sum), StaticMethod(typeof(DIOSerdeBigInteger), "readBigDec", input));
            }
        }
    }
} // end of namespace