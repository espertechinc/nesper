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
    public class PollResultIndexingStrategyInKeywordMulti : PollResultIndexingStrategy
    {
        private PropertyHashedEventTableFactory[] factories;

        public int StreamNum { get; set; }

        public string[] PropertyNames { get; set; }

        public EventPropertyValueGetter[] ValueGetters { get; set; }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext)
        {
            if (!isActiveCache) {
                return new EventTable[] {new UnindexedEventTableList(pollResult, StreamNum)};
            }

            var tables = new EventTable[ValueGetters.Length];
            for (var i = 0; i < ValueGetters.Length; i++) {
                tables[i] = factories[i].MakeEventTables(agentInstanceContext, null)[0];
                tables[i].Add(pollResult.ToArray(), agentInstanceContext);
            }

            return tables;
        }

        public void Init()
        {
            factories = new PropertyHashedEventTableFactory[ValueGetters.Length];
            for (var i = 0; i < PropertyNames.Length; i++) {
                factories[i] = new PropertyHashedEventTableFactory(
                    StreamNum,
                    new[] {PropertyNames[i]},
                    false,
                    null,
                    ValueGetters[i],
                    null);
            }
        }
    }
} // end of namespace