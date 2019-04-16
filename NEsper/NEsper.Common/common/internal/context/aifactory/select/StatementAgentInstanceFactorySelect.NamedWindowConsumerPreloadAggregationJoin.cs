///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.@join.@base;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.context.aifactory.@select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreloadAggregationJoin : StatementAgentInstancePreload
        {
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly ResultSetProcessor resultSetProcessor;

            public NamedWindowConsumerPreloadAggregationJoin(
                JoinPreloadMethod joinPreloadMethod,
                ResultSetProcessor resultSetProcessor)
            {
                this.joinPreloadMethod = joinPreloadMethod;
                this.resultSetProcessor = resultSetProcessor;
            }

            public void ExecutePreload()
            {
                joinPreloadMethod.PreloadAggregation(resultSetProcessor);
            }
        }
    }
}