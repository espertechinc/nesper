///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValuePairInKWMultiIdx
    {
        public QueryGraphValuePairInKWMultiIdx(IList<ExprNode> indexed, QueryGraphValueEntryInKeywordMultiIdx key)
        {
            Indexed = indexed;
            Key = key;
        }

        public IList<ExprNode> Indexed { get; private set; }

        public QueryGraphValueEntryInKeywordMultiIdx Key { get; private set; }
    }
}