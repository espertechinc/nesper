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
    public class GroupByRollupEvalContext
    {
        private readonly IDictionary<ExprNode, int> _expressionToIndex;

        public GroupByRollupEvalContext(IDictionary<ExprNode, int> expressionToIndex)
        {
            _expressionToIndex = expressionToIndex;
        }

        public int GetIndex(ExprNode node)
        {
            return _expressionToIndex.Get(node);
        }
    }
}