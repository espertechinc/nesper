///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupNodeSingleExpr : GroupByRollupNodeBase
    {
        private readonly ExprNode _expression;

        public GroupByRollupNodeSingleExpr(ExprNode expression)
        {
            _expression = expression;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            int index = context.GetIndex(_expression);
            return Collections.SingletonList(
                new int[]
                {
                    index
                });
        }
    }
}