///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;
using com.espertech.esper.runtime.@internal.kernel.stage;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
	/// <summary>
	///     Timer unit for multiple callbacks for a statement.
	/// </summary>
	public class TimerUnitMultipleStaged : TimerUnit
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object callbackObject;
        private readonly EPStatementAgentInstanceHandle handle;
        private readonly EPStageEventServiceImpl runtime;

        private readonly StageSpecificServices services;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="services">runtime services</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="handle">statement handle</param>
        /// <param name="callbackObject">callback list</param>
        public TimerUnitMultipleStaged(
            StageSpecificServices services,
            EPStageEventServiceImpl runtime,
            EPStatementAgentInstanceHandle handle,
            object callbackObject)
        {
            this.services = services;
            this.handle = handle;
            this.runtime = runtime;
            this.callbackObject = callbackObject;
        }

        public void Run()
        {
            try {
                EPEventServiceHelper.ProcessStatementScheduleMultiple(handle, callbackObject, services);

                // Let listeners know of results
                runtime.Dispatch();

                // Work off the event queue if any events accumulated in there via a route()
                runtime.ProcessThreadWorkQueue();
            }
            catch (Exception e) {
                log.Error("Unexpected error processing multiple timer execution: " + e.Message, e);
            }
        }
    }
} // end of namespace