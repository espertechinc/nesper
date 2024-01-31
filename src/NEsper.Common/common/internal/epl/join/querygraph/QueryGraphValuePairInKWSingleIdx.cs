///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.join.querygraph
{
    public class QueryGraphValuePairInKWSingleIdx
    {
        public QueryGraphValuePairInKWSingleIdx(
            string[] indexed,
            IList<QueryGraphValueEntryInKeywordSingleIdx> key)
        {
            Indexed = indexed;
            Key = key;
        }

        public string[] Indexed { get; }

        public IList<QueryGraphValueEntryInKeywordSingleIdx> Key { get; }
    }
} // end of namespace