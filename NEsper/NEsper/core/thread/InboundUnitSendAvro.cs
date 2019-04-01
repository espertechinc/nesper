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
    /// <summary>Inbound work unit processing a map event.</summary>
    public class InboundUnitSendAvro
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Object _genericRecordDotData;
        private readonly string _eventTypeName;
        private readonly EPServicesContext _services;
        private readonly EPRuntimeImpl _runtime;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="genericRecordDotData">to send</param>
        /// <param name="eventTypeName">type name</param>
        /// <param name="services">to wrap</param>
        /// <param name="runtime">to process</param>
        public InboundUnitSendAvro(
            Object genericRecordDotData,
            string eventTypeName,
            EPServicesContext services,
            EPRuntimeImpl runtime)
        {
            this._eventTypeName = eventTypeName;
            this._genericRecordDotData = genericRecordDotData;
            this._services = services;
            this._runtime = runtime;
        }

        public void Run()
        {
            try
            {
                EventBean eventBean = _services.EventAdapterService.AdapterForAvro(
                    _genericRecordDotData, _eventTypeName);
                _runtime.ProcessWrappedEvent(eventBean);
            }
            catch (Exception e)
            {
                Log.Error("Unexpected error processing Object-array event: " + e.Message, e);
            }
        }
    }
} // end of namespace
