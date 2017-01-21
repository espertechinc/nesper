///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    [Serializable]
    public class QueryGraphValueEntryInKeywordSingleIdx : QueryGraphValueEntry
    {
        public QueryGraphValueEntryInKeywordSingleIdx(ExprNode[] keyExprs)
        {
            KeyExprs = keyExprs;
        }

        public ExprNode[] KeyExprs { get; private set; }

        public String ToQueryPlan()
        {
            return "in-keyword single-indexed multiple key lookup " + ExprNodeUtility.ToExpressionStringMinPrecedence(KeyExprs);
        }
    }
    
}
