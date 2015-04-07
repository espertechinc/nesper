///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification of matches available.
    /// </summary>
    public class MatchEventSpec
    {
        public MatchEventSpec(IDictionary<String, Pair<EventType, String>> taggedEventTypes, IDictionary<String, Pair<EventType, String>> arrayEventTypes)
        {
            TaggedEventTypes = taggedEventTypes;
            ArrayEventTypes = arrayEventTypes;
        }
    
        public MatchEventSpec()
        {
            TaggedEventTypes = new LinkedHashMap<String, Pair<EventType, String>>();
            ArrayEventTypes = new LinkedHashMap<String, Pair<EventType, String>>();
        }

        public IDictionary<string, Pair<EventType, string>> ArrayEventTypes { get; private set; }

        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes { get; private set; }
    }
}
