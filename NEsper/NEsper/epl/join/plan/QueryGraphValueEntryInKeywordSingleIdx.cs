///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.plan
{
    [Serializable]
    public class QueryGraphValueEntryInKeywordSingleIdx : QueryGraphValueEntry
    {
        public QueryGraphValueEntryInKeywordSingleIdx(IList<ExprNode> keyExprs)
        {
            KeyExprs = keyExprs;
        }

        public IList<ExprNode> KeyExprs { get; private set; }

        public String ToQueryPlan()
        {
            return "in-keyword single-indexed multiple key lookup " + ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(KeyExprs);
        }
    }
    
}
