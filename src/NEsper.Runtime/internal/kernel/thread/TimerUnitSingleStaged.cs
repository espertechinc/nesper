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
    ///     Timer unit for a single callback for a statement.
    /// </summary>
    public class TimerUnitSingleStaged : TimerUnit
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly EPStatementHandleCallbackSchedule _handleCallback;
        private readonly EPStageEventServiceImpl _runtime;

        private readonly StageSpecificServices _services;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="services">runtime services</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="handleCallback">callback</param>
        public TimerUnitSingleStaged(
            StageSpecificServices services,
            EPStageEventServiceImpl runtime,
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