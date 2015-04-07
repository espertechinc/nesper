///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// <summary>Inbound work unit processing a map event. </summary>
    public class InboundUnitSendObjectArray
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Object[] _properties;
        private readonly String _eventTypeName;
        private readonly EPServicesContext _services;
        private readonly EPRuntimeImpl _runtime;
    
        /// <summary>Ctor. </summary>
        /// <param name="properties">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="services">to wrap</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendObjectArray(Object[] properties, String eventTypeName, EPServicesContext services, EPRuntimeImpl runtime)
        {
            _eventTypeName = eventTypeName;
            _properties = properties;
            _services = services;
            _runtime = runtime;
        }
    
        public void Run()
        {
            try
            {
                EventBean eventBean = _services.EventAdapterService.AdapterForObjectArray(_properties, _eventTypeName);
                _runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
}
