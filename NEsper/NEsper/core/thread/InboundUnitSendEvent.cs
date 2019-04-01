///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>Inbound unit for unwrapped events. </summary>
    public class InboundUnitSendEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Object _theEvent;
        private readonly EPRuntimeImpl _runtime;

        /// <summary>Ctor. </summary>
        /// <param name="theTheEvent">to process</param>
        /// <param name="runtime">to process event</param>
        public InboundUnitSendEvent(Object theTheEvent, EPRuntimeImpl runtime)
        {
            _theEvent = theTheEvent;
            _runtime = runtime;
        }

        public void Run()
        {
            try
            {
                _runtime.ProcessEvent(_theEvent);
            }
            catch (Exception e)
            {
                _runtime.ExceptionHandlingService.HandleInboundPoolException(_runtime.EngineURI, e, _theEvent);
                Log.Error("Unexpected error processing unwrapped event: " + e.Message, e);
            }
        }
    }
}
