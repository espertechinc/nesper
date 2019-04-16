///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByColumnForge
    {
        public AggregationLocalGroupByColumnForge(
            bool defaultGroupLevel,
            ExprForge[] partitionForges,
            int methodOffset,
            bool methodAgg,
            AggregationAccessorSlotPairForge pair,
            int levelNum)
        {
            IsDefaultGroupLevel = defaultGroupLevel;
            PartitionForges = partitionForges;
            MethodOffset = methodOffset;
            IsMethodAgg = methodAgg;
            Pair = pair;
            LevelNum = levelNum;
        }

        public ExprForge[] PartitionForges { get; }

        public int MethodOffset { get; }

        public bool IsDefaultGroupLevel { get; }

        public bool IsMethodAgg { get; }

        public AggregationAccessorSlotPairForge Pair { get; }

        public int LevelNum { get; }

        public CodegenExpression ToExpression(int fieldNum)
        {
            return NewInstance(
                typeof(AggregationLocalGroupByColumn), Constant(IsDefaultGroupLevel), Constant(fieldNum),
                Constant(LevelNum));
        }
    }
} // end of namespace