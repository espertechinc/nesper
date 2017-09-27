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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>Route execution work unit.</summary>
    public class RouteUnitMultiple
    {
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntimeImpl _epRuntime;
        private readonly EventBean _theEvent;
        private readonly long _filterVersion;
        private readonly Object _callbackList;
        private readonly EPStatementAgentInstanceHandle _handle;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epRuntime">runtime to process</param>
        /// <param name="callbackList">callback list</param>
        /// <param name="theEvent">event to pass</param>
        /// <param name="handle">statement handle</param>
        /// <param name="filterVersion">version of filter</param>
        public RouteUnitMultiple(
            EPRuntimeImpl epRuntime,
            Object callbackList,
            EventBean theEvent,
            EPStatementAgentInstanceHandle handle,
            long filterVersion)
        {
            _epRuntime = epRuntime;
            _callbackList = callbackList;
            _theEvent = theEvent;
            _handle = handle;
            _filterVersion = filterVersion;
        }

        public void Run()
        {
            try
            {
                _epRuntime.ProcessStatementFilterMultiple(_handle, _callbackList, _theEvent, _filterVersion);

                _epRuntime.Dispatch();

                _epRuntime.ProcessThreadWorkQueue();
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing multiple route execution: " + e.Message, e);
            }
        }
    }
} // end of namespace
