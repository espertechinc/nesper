///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.agg.method.core.AggregatorCodegenUtil;
using static com.espertech.esper.common.@internal.epl.agg.method.sum.AggregationFactoryMethodSum;

namespace com.espertech.esper.common.@internal.epl.agg.method.sum
{
    public class AggregatorSumNonBig : AggregatorSumBase
    {
        public AggregatorSumNonBig(
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
                factory,
                col,
                rowCtor,
                membersColumnized,
                classScope,
                optionalDistinctValueType,
                hasFilter,
                optionalFilter,
                sumType)

        {
            ISet<Type> typeSet = Collections.Set(
                typeof(double?),
                typeof(long?),
                typeof(int?),
                typeof(float?));

            if (!typeSet.Contains(sumType)) {
                throw new ArgumentException("Invalid sum type " + sumType);
            }
        }

        protected override CodegenExpression InitOfSum()
        {
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
            if (sumType == typeof(double?)) {
                method.Block.Apply(WriteDouble(output, row, sum));
            }
            else if (sumType == typeof(long?)) {
                method.Block.Apply(WriteLong(output, row, sum));
            }
            else if (sumType == typeof(int?)) {
                method.Block.Apply(WriteInt(output, row, sum));
            }
            else if (sumType == typeof(float?)) {
                method.Block.Apply(WriteFloat(output, row, sum));
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
            if (sumType == typeof(double?)) {
                method.Block.Apply(ReadDouble(row, sum, input));
            }
            else if (sumType == typeof(long?)) {
                method.Block.Apply(ReadLong(row, sum, input));
            }
            else if (sumType == typeof(int?)) {
                method.Block.Apply(ReadInt(row, sum, input));
            }
            else if (sumType == typeof(float?)) {
                method.Block.Apply(ReadFloat(row, sum, input));
            }
            else {
                throw new IllegalStateException("Unrecognized sum type " + sumType);
            }
        }

        private void ApplyAgg(
            bool enter,
            CodegenExpressionRef value,
            Type valueType,
            CodegenMethod method)
        {
            var coercer = GetCoercerNonBigIntDec(valueType);
            method.Block.AssignRef(sum, Op(sum, enter ? "+" : "-", coercer.CoerceCodegen(value, valueType)));
        }

        private void ApplyTable(
            bool enter,
            CodegenExpressionRef value,
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block.AssignRef(sum, Op(sum, enter ? "+" : "-", Cast(sumType, value)));
        }
    }
} // end of namespace