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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.core.thread
{
    /// <summary>
    /// Timer unit for a single callback for a statement.
    /// </summary>
    public class TimerUnitSingle
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPStatementHandleCallback _handleCallback;
        private readonly EPRuntimeImpl _runtime;
        private readonly EPServicesContext _services;

        /// <summary>Ctor. </summary>
        /// <param name="services">engine services</param>
        /// <param name="runtime">runtime to process</param>
        /// <param name="handleCallback">callback</param>
        public TimerUnitSingle(EPServicesContext services, EPRuntimeImpl runtime, EPStatementHandleCallback handleCallback)
        {
            _services = services;
            _runtime = runtime;
            _handleCallback = handleCallback;
        }

        public void Run()
        {
            try
            {
                EPRuntimeImpl.ProcessStatementScheduleSingle(_handleCallback, _services);

                _runtime.Dispatch();

                _runtime.ProcessThreadWorkQueue();
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing timer execution: " + e.Message, e);
            }
        }
    }
}