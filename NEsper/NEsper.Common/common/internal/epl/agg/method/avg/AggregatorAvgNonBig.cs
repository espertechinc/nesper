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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.method.sum;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.method.avg
{
    /// <summary>
    ///     Average that generates double-typed numbers.
    /// </summary>
    public class AggregatorAvgNonBig : AggregatorSumNonBig
    {
        public AggregatorAvgNonBig(
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
        }

        public override void GetValueCodegen(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            method.Block
                .IfCondition(EqualsIdentity(cnt, Constant(0)))
                .BlockReturn(ConstantNull());
            if (sumType == typeof(double)) {
                method.Block.MethodReturn(Op(sum, "/", cnt));
            }
            else {
                method.Block.MethodReturn(Op(sum, "/", Cast(typeof(double), cnt)));
            }
        }
    }
} // end of namespace