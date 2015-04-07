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
    public class GroupByRollupNodeCombinedExpr : GroupByRollupNodeBase
    {
        private readonly IList<ExprNode> _expressions;

        public GroupByRollupNodeCombinedExpr(IList<ExprNode> expressions)
        {
            _expressions = expressions;
        }

        public override IList<int[]> Evaluate(GroupByRollupEvalContext context)
        {
            var result = new int[_expressions.Count];
            for (int i = 0; i < _expressions.Count; i++)
            {
                int index = context.GetIndex(_expressions[i]);
                result[i] = index;
            }

            result.SortInPlace();

            // find dups
            for (int i = 0; i < result.Length - 1; i++)
            {
                if (result[i] == result[i + 1])
                {
                    throw new GroupByRollupDuplicateException(
                        new int[]
                        {
                            result[i]
                        });
                }
            }

            return Collections.SingletonList(result);
        }
    }
}