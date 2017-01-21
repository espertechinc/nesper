///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValuePairInKWSingleIdx
    {
        public QueryGraphValuePairInKWSingleIdx(String[] indexed, IList<QueryGraphValueEntryInKeywordSingleIdx> key)
        {
            Indexed = indexed;
            Key = key;
        }

        public string[] Indexed { get; private set; }

        public IList<QueryGraphValueEntryInKeywordSingleIdx> Key { get; private set; }
    }
}