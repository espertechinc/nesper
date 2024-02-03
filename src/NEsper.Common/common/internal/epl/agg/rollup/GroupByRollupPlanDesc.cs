///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupPlanDesc
    {
        public GroupByRollupPlanDesc(
            ExprNode[] expressions,
            AggregationGroupByRollupDescForge rollupDesc)
        {
            Expressions = expressions;
            RollupDesc = rollupDesc;
        }

        public ExprNode[] Expressions { get; }

        public AggregationGroupByRollupDescForge RollupDesc { get; }
    }
} // end of namespace