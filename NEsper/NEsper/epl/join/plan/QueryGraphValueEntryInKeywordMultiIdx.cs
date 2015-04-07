///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class QueryGraphValueEntryInKeywordMultiIdx : QueryGraphValueEntry
    {
        public QueryGraphValueEntryInKeywordMultiIdx(ExprNode keyExpr)
        {
            KeyExpr = keyExpr;
        }

        public ExprNode KeyExpr { get; private set; }

        public String ToQueryPlan()
        {
            return "in-keyword multi-indexed single keyed lookup " + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(KeyExpr);
        }
    }
    
}
