///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.composite;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyComposite : PollResultIndexingStrategy
    {
        public int StreamNum { get; set; }

        public string[] OptionalKeyedProps { get; set; }

        public Type[] OptKeyCoercedTypes { get; set; }

        public EventPropertyValueGetter HashGetter { get; set; }

        public string[] RangeProps { get; set; }

        public Type[] OptRangeCoercedTypes { get; set; }

        public EventPropertyValueGetter[] RangeGetters { get; set; }

        public PropertyCompositeEventTableFactory Factory { get; set; }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext)
        {
            if (!isActiveCache) {
                return new EventTable[] {new UnindexedEventTableList(pollResult, StreamNum)};
            }

            var tables = Factory.MakeEventTables(agentInstanceContext, null);
            foreach (var table in tables) {
                table.Add(pollResult.ToArray(), agentInstanceContext);
            }

            return tables;
        }

        public void Init()
        {
            Factory = new PropertyCompositeEventTableFactory(
                StreamNum,
                OptionalKeyedProps,
                OptKeyCoercedTypes,
                HashGetter,
                RangeProps,
                OptRangeCoercedTypes,
                RangeGetters);
        }
    }
} // end of namespace