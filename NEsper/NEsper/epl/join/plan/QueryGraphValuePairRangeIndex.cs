///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValuePairRangeIndex
    {
        public QueryGraphValuePairRangeIndex(IList<string> indexed, IList<QueryGraphValueEntryRange> key)
        {
            Indexed = indexed;
            Keys = key;
        }

        public IList<string> Indexed { get; private set; }

        public IList<QueryGraphValueEntryRange> Keys { get; private set; }
    }
}