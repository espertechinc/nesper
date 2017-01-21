///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    public class GroupByClauseExpressions
    {
        public GroupByClauseExpressions(ExprNode[] groupByNodes)
            : this(groupByNodes, null, null, null, null)
        {
        }

        public GroupByClauseExpressions(
            ExprNode[] groupByNodes,
            int[][] groupByRollupLevels,
            ExprNode[][] selectClauseCopy,
            ExprNode[] optHavingNodeCopy,
            ExprNode[][] optOrderByPerLevel)
        {
            GroupByNodes = groupByNodes;
            GroupByRollupLevels = groupByRollupLevels;
            SelectClausePerLevel = selectClauseCopy;
            OptHavingNodePerLevel = optHavingNodeCopy;
            OptOrderByPerLevel = optOrderByPerLevel;
        }

        public int[][] GroupByRollupLevels { get; private set; }

        public ExprNode[][] SelectClausePerLevel { get; private set; }

        public ExprNode[][] OptOrderByPerLevel { get; private set; }

        public ExprNode[] OptHavingNodePerLevel { get; private set; }

        public ExprNode[] GroupByNodes { get; private set; }
    }
}