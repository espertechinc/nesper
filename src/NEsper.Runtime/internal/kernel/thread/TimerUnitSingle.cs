///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.@internal.kernel.thread
{
    /// <summary>
    ///     Timer unit for a single callback for a statement.
    /// </summary>
    public class TimerUnitSingle : TimerUnit
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TimerUnitSingle));

        private readonly EPStatementHandleCallbackSchedule _handleCallback;
        private readonly EPEventServiceImpl _runtime;
        private readonly EPServicesContext _services;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="services">runtime services</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="handleCallback">callback</param>
        public TimerUnitSingle(
            EPServicesContext services,
            EPEventServiceImpl runtime,
            EPStatementHandleCallbackSchedule handleCallback)
        {
            this._services = services;
            this._runtime = runtime;
            this._handleCallback = handleCallback;
        }

        public void Run()
        {
            try {
                EPEventServiceHelper.ProcessStatementScheduleSingle(_handleCallback, _services);

                _runtime.Dispatch();

                _runtime.ProcessThreadWorkQueue();
            }
            catch (Exception e) {
                Log.Error("Unexpected error processing timer execution: " + e.Message, e);
            }
        }
    }
} // end of namespace