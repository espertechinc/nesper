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
    public class GroupByRollupEvalContext
    {
        private readonly IDictionary<ExprNode, int> expressionToIndex;

        public GroupByRollupEvalContext(IDictionary<ExprNode, int> expressionToIndex)
        {
            this.expressionToIndex = expressionToIndex;
        }

        public int GetIndex(ExprNode node)
        {
            return expressionToIndex.Get(node);
        }
    }
} // end of namespace