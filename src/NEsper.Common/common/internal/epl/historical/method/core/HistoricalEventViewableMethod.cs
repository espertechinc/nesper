///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.execstrategy;

namespace com.espertech.esper.common.@internal.epl.historical.method.core
{
    public class HistoricalEventViewableMethod : HistoricalEventViewableBase
    {
        public HistoricalEventViewableMethod(
            HistoricalEventViewableMethodFactory factory,
            PollExecStrategy pollExecStrategy,
            AgentInstanceContext agentInstanceContext)
            : base(factory, pollExecStrategy, agentInstanceContext)

        {
            try {
                ConfigurationCommonMethodRef configCache =
                    agentInstanceContext.ImportServiceRuntime.GetConfigurationMethodRef(factory.ConfigurationName);
                ConfigurationCommonCache dataCacheDesc = configCache != null ? configCache.DataCacheDesc : null;
                DataCache = agentInstanceContext.HistoricalDataCacheFactory.GetDataCache(
                    dataCacheDesc,
                    agentInstanceContext,
                    factory.StreamNumber,
                    factory.ScheduleCallbackId);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw new EPException("Failed to obtain cache: " + e.Message, e);
            }
        }
    }
} // end of namespace