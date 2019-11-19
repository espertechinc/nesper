///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class GroupByRollupNodeCombinedExpr : GroupByRollupNodeBase
    {
        private readonly IList<ExprNode> expressions;

        public GroupByRollupNodeCombinedExpr(IList<ExprNode> expressions)
        {
            this.expressions = expressions;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            var result = new int[expressions.Count];
            for (var i = 0; i < expressions.Count; i++) {
                int index = context.GetIndex(expressions[i]);
                result[i] = index;
            }

            Collections.SortInPlace(result);

            // find dups
            for (var i = 0; i < result.Length - 1; i++) {
                if (result[i] == result[i + 1]) {
                    throw new GroupByRollupDuplicateException(new[] {result[i]});
                }
            }

            return Collections.SingletonList(result);
        }
    }
} // end of namespace