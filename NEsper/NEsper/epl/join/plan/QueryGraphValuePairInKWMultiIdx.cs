///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValuePairInKWMultiIdx
    {
        public QueryGraphValuePairInKWMultiIdx(ExprNode[] indexed, QueryGraphValueEntryInKeywordMultiIdx key)
        {
            Indexed = indexed;
            Key = key;
        }

        public ExprNode[] Indexed { get; private set; }

        public QueryGraphValueEntryInKeywordMultiIdx Key { get; private set; }
    }
}