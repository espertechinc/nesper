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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly object _callbackObject;
        private readonly EPStatementAgentInstanceHandle _handle;
        private readonly EPStageEventServiceImpl _runtime;

        private readonly StageSpecificServices _services;

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
            this._services = services;
            this._handle = handle;
            this._runtime = runtime;
            this._callbackObject = callbackObject;
        }

        public void Run()
        {
            try {
                EPEventServiceHelper.ProcessStatementScheduleMultiple(_handle, _callbackObject, _services);

                // Let listeners know of results
                _runtime.Dispatch();

                // Work off the event queue if any events accumulated in there via a route()
                _runtime.ProcessThreadWorkQueue();
            }
            catch (Exception e) {
                Log.Error("Unexpected error processing multiple timer execution: " + e.Message, e);
            }
        }
    }
} // end of namespace