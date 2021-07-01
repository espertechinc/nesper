///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification of matches available.
    /// </summary>
    public class MatchEventSpec
    {
        public MatchEventSpec(
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes)
        {
            TaggedEventTypes = taggedEventTypes;
            ArrayEventTypes = arrayEventTypes;
        }

        public MatchEventSpec()
        {
            TaggedEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
            ArrayEventTypes = new LinkedHashMap<string, Pair<EventType, string>>();
        }

        public IDictionary<string, Pair<EventType, string>> ArrayEventTypes { get; }

        public IDictionary<string, Pair<EventType, string>> TaggedEventTypes { get; }
    }
}