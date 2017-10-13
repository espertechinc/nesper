///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>Route unit for single match.</summary>
    public class RouteUnitSingle
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntimeImpl _epRuntime;
        private readonly EventBean _theEvent;
        private readonly long _filterVersion;
        private readonly EPStatementHandleCallback _handleCallback;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epRuntime">runtime to process</param>
        /// <param name="handleCallback">callback</param>
        /// <param name="theEvent">event</param>
        /// <param name="filterVersion">version of filter</param>
        public RouteUnitSingle(
            EPRuntimeImpl epRuntime,
            EPStatementHandleCallback handleCallback,
            EventBean theEvent,
            long filterVersion)
        {
            _epRuntime = epRuntime;
            _theEvent = theEvent;
            _handleCallback = handleCallback;
            _filterVersion = filterVersion;
        }

        public void Run()
        {
            try
            {
                _epRuntime.ProcessStatementFilterSingle(
                    _handleCallback.AgentInstanceHandle, _handleCallback, _theEvent, _filterVersion);

                _epRuntime.Dispatch();

                _epRuntime.ProcessThreadWorkQueue();
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing route execution: " + e.Message, e);
            }
        }
    }
} // end of namespace
