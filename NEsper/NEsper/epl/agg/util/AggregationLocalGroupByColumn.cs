///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.util
{
    public class AggregationLocalGroupByColumn
    {
        public AggregationLocalGroupByColumn(
            bool defaultGroupLevel,
            ExprEvaluator[] partitionEvaluators,
            int methodOffset,
            bool methodAgg,
            AggregationAccessorSlotPair pair,
            int levelNum)
        {
            IsDefaultGroupLevel = defaultGroupLevel;
            PartitionEvaluators = partitionEvaluators;
            MethodOffset = methodOffset;
            IsMethodAgg = methodAgg;
            Pair = pair;
            LevelNum = levelNum;
        }

        public ExprEvaluator[] PartitionEvaluators { get; private set; }

        public int MethodOffset { get; private set; }

        public bool IsDefaultGroupLevel { get; private set; }

        public bool IsMethodAgg { get; private set; }

        public AggregationAccessorSlotPair Pair { get; private set; }

        public int LevelNum { get; private set; }
    }
} // end of namespace