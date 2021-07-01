///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyAllUnfilteredFactory : SubordWMatchExprLookupStrategyFactory
    {
        public SubordTableLookupStrategyFactory OptionalInnerStrategy => null;

        public SubordWMatchExprLookupStrategy Realize(
            EventTable[] indexes,
            AgentInstanceContext agentInstanceContext,
            IEnumerable<EventBean> scanIterable,
            VirtualDWView virtualDataWindow)
        {
            return new SubordWMatchExprLookupStrategyAllUnfiltered(scanIterable);
        }

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace