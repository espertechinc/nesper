///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>
    /// Inbound unit for wrapped events.
    /// </summary>
    public class InboundUnitSendWrapped 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EventBean _eventBean;
        private readonly EPRuntimeEventSender _runtime;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="theEvent">inbound event, wrapped</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendWrapped(EventBean theEvent, EPRuntimeEventSender runtime)
        {
            _eventBean = theEvent;
            _runtime = runtime;
        }
    
        public void Run()
        {
            try
            {
                _runtime.ProcessWrappedEvent(_eventBean);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing wrapped event: " + e.Message, e);
            }
        }
    }
} // end of namespace
