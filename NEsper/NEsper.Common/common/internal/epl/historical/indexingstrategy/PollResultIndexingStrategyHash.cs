///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    public class PollResultIndexingStrategyHash : PollResultIndexingStrategy
    {
        private PropertyHashedEventTableFactory factory;

        public int StreamNum { get; set; }

        public string[] PropertyNames { get; set; }

        public EventPropertyValueGetter ValueGetter { get; set; }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext)
        {
            if (!isActiveCache) {
                return new EventTable[] {new UnindexedEventTableList(pollResult, StreamNum)};
            }

            var tables = factory.MakeEventTables(agentInstanceContext, null);
            foreach (var table in tables) {
                table.Add(pollResult.ToArray(), agentInstanceContext);
            }

            return tables;
        }

        public void Init()
        {
            factory = new PropertyHashedEventTableFactory(StreamNum, PropertyNames, false, null, ValueGetter);
        }
    }
} // end of namespace