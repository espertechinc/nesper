///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValuePairHashKeyIndex
    {
        public QueryGraphValuePairHashKeyIndex(IList<string> indexed, IList<QueryGraphValueEntryHashKeyed> key, IList<string> strictKeys)
        {
            Indexed = indexed;
            Keys = key;
            StrictKeys = strictKeys;
        }

        public IList<string> Indexed { get; private set; }

        public IList<QueryGraphValueEntryHashKeyed> Keys { get; private set; }

        public IList<string> StrictKeys { get; private set; }
    }
}
