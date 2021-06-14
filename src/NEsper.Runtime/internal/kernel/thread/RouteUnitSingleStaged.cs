///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Route unit for single match.
    /// </summary>
    public class RouteUnitSingleStaged : RouteUnitRunnable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStageEventServiceImpl epRuntime;
        private readonly long filterVersion;
        private readonly EPStatementHandleCallbackFilter handleCallback;
        private readonly EventBean theEvent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="epRuntime">runtime to process</param>
        /// <param name="handleCallback">callback</param>
        /// <param name="theEvent">event</param>
        /// <param name="filterVersion">version of filter</param>
        public RouteUnitSingleStaged(
            EPStageEventServiceImpl epRuntime,
            EPStatementHandleCallbackFilter handleCallback,
            EventBean theEvent,
            long filterVersion)
        {
            this.epRuntime = epRuntime;
            this.theEvent = theEvent;
            this.handleCallback = handleCallback;
            this.filterVersion = filterVersion;
        }

        public void Run()
        {
            try {
                epRuntime.ProcessStatementFilterSingle(handleCallback.AgentInstanceHandle, handleCallback, theEvent, filterVersion, 0);

                epRuntime.Dispatch();

                epRuntime.ProcessThreadWorkQueue();
            }
            catch (Exception e) {
                log.Error("Unexpected error processing route execution: " + e.Message, e);
            }
        }
    }
} // end of namespace