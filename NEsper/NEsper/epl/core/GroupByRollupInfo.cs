///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.core
{
    public class GroupByRollupInfo
    {
        public ExprNode[] ExprNodes { get; private set; }

        public AggregationGroupByRollupDesc RollupDesc { get; private set; }

        public GroupByRollupInfo(ExprNode[] exprNodes, AggregationGroupByRollupDesc rollupDesc)
        {
            ExprNodes = exprNodes;
            RollupDesc = rollupDesc;
        }
    }
} // end of namespace