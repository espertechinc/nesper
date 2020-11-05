///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValuePairInKWSingleIdxForge
    {
        public QueryGraphValuePairInKWSingleIdxForge(
            string[] indexed,
            IList<QueryGraphValueEntryInKeywordSingleIdxForge> key)
        {
            Indexed = indexed;
            Key = key;
        }

        public string[] Indexed { get; }

        public IList<QueryGraphValueEntryInKeywordSingleIdxForge> Key { get; }
    }
} // end of namespace