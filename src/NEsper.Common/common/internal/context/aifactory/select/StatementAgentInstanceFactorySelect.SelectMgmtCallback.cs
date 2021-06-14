///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        public class SelectMgmtCallback : AgentInstanceMgmtCallback
        {
            private readonly ResultSetProcessor _resultSetProcessor;
            private readonly AggregationService _aggregationService;

            public SelectMgmtCallback(Pair<ResultSetProcessor, AggregationService> processorPair)
            {
                _resultSetProcessor = processorPair.First;
                _aggregationService = processorPair.Second;
            }

            public void Stop(AgentInstanceStopServices services)
            {
                _resultSetProcessor.Stop();
                _aggregationService.Stop();
            }

            public void Transfer(AgentInstanceTransferServices services)
            {
                // no action
            }
        }
    }
}