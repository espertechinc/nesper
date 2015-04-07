///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Xml;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    /// <summary>Inbound unit for DOM events. </summary>
    public class InboundUnitSendDOM
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly XmlNode _event;
        private readonly EPRuntimeImpl _runtime;
        private readonly EPServicesContext _services;

        /// <summary>Ctor. </summary>
        /// <param name="theEvent">document</param>
        /// <param name="services">for wrapping event</param>
        /// <param name="runtime">runtime to process</param>
        public InboundUnitSendDOM(XmlNode theEvent,
                                  EPServicesContext services,
                                  EPRuntimeImpl runtime)
        {
            _event = theEvent;
            _services = services;
            _runtime = runtime;
        }

        public void Run()
        {
            try
            {
                EventBean eventBean = _services.EventAdapterService.AdapterForDOM(_event);
                _runtime.ProcessEvent(eventBean);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing DOM event: " + e.Message, e);
            }
        }
    }
}