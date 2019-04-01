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
    public class QueryGraphValuePairRangeIndex
    {
        public QueryGraphValuePairRangeIndex(string[] indexed, IList<QueryGraphValueEntryRange> key)
        {
            Indexed = indexed;
            Keys = key;
        }

        public string[] Indexed { get; }

        public IList<QueryGraphValueEntryRange> Keys { get; }
    }
} // end of namespace