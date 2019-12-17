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
    public class QueryGraphValuePairHashKeyIndexForge
    {
        public QueryGraphValuePairHashKeyIndexForge(
            string[] indexed,
            IList<QueryGraphValueEntryHashKeyedForge> key,
            string[] strictKeys)
        {
            Indexed = indexed;
            Keys = key;
            StrictKeys = strictKeys;
        }

        public string[] Indexed { get; }

        public IList<QueryGraphValueEntryHashKeyedForge> Keys { get; }

        public string[] StrictKeys { get; }
    }
} // end of namespace