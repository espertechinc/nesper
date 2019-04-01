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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodConversionStrategyScript : MethodConversionStrategyBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MethodConversionStrategyScript));

        public override IList<EventBean> Convert(
            object invocationResult,
            MethodTargetStrategy origin,
            AgentInstanceContext agentInstanceContext)
        {
            if (!(invocationResult is EventBean[])) {
                string result = invocationResult == null
                    ? "null"
                    : invocationResult.GetType().Name;
                Log.Warn("Script expected return type EventBean[] does not match result " + result);
                return Collections.GetEmptyList<EventBean>();
            }

            return invocationResult.UnwrapIntoList<EventBean>();
        }
    }
} // end of namespace