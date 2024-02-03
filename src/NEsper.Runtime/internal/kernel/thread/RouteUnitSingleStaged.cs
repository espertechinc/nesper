///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStageEventServiceImpl _epRuntime;
        private readonly long _filterVersion;
        private readonly EPStatementHandleCallbackFilter _handleCallback;
        private readonly EventBean _theEvent;

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
            this._epRuntime = epRuntime;
            this._theEvent = theEvent;
            this._handleCallback = handleCallback;
            this._filterVersion = filterVersion;
        }

        public void Run()
        {
            try {
                _epRuntime.ProcessStatementFilterSingle(_handleCallback.AgentInstanceHandle, _handleCallback, _theEvent, _filterVersion, 0);

                _epRuntime.Dispatch();

                _epRuntime.ProcessThreadWorkQueue();
            }
            catch (Exception e) {
                Log.Error("Unexpected error processing route execution: " + e.Message, e);
            }
        }
    }
} // end of namespace