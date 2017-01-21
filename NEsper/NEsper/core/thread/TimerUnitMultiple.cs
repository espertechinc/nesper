///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.core.thread
{
    /// <summary>
    /// Timer unit for multiple callbacks for a statement.
    /// </summary>
    public class TimerUnitMultiple
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Object _callbackObject;
        private readonly EPStatementAgentInstanceHandle _handle;
        private readonly EPRuntimeImpl _runtime;
        private readonly EPServicesContext _services;

        /// <summary>Ctor. </summary>
        /// <param name="services">engine services</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackObject">callback list</param>
        public TimerUnitMultiple(EPServicesContext services, EPRuntimeImpl runtime, EPStatementAgentInstanceHandle handle, object callbackObject)
        {
            _services = services;
            _handle = handle;
            _runtime = runtime;
            _callbackObject = callbackObject;
        }

        public void Run()
        {
            try
            {
                EPRuntimeImpl.ProcessStatementScheduleMultiple(_handle, _callbackObject, _services);

                // Let listeners know of results
                _runtime.Dispatch();

                // Work off the event queue if any events accumulated in there via a Route()
                _runtime.ProcessThreadWorkQueue();
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing multiple timer execution: " + e.Message, e);
            }
        }
    }
}