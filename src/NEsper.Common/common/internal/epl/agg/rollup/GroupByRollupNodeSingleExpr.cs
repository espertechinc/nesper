///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupNodeSingleExpr : GroupByRollupNodeBase
    {
        private readonly ExprNode expression;

        public GroupByRollupNodeSingleExpr(ExprNode expression)
        {
            this.expression = expression;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            var index = context.GetIndex(expression);
            return Collections.SingletonList(new[] { index });
        }
    }
} // end of namespace