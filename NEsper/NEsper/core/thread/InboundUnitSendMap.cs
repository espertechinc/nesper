///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.core.thread
{
    using DataMap = IDictionary<string, object>;

    /// <summary>Inbound work unit processing a map event. </summary>
    public class InboundUnitSendMap
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly String _eventTypeName;
        private readonly DataMap _map;
        private readonly EPRuntimeImpl _runtime;
        private readonly EPServicesContext _services;

        /// <summary>Ctor. </summary>
        /// <param name="map">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="services">to wrap</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendMap(DataMap map,
                                  String eventTypeName,
                                  EPServicesContext services,
                                  EPRuntimeImpl runtime)
        {
            _eventTypeName = eventTypeName;
            _map = map;
            _services = services;
            _runtime = runtime;
        }

        public void Run()
        {
            try
            {
                EventBean eventBean = _services.EventAdapterService.AdapterForMap(_map, _eventTypeName);
                _runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing Map event: " + e.Message, e);
            }
        }
    }
}