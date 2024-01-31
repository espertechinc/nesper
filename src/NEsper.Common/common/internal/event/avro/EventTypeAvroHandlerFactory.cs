///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.avro
{
    public class EventTypeAvroHandlerFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static EventTypeAvroHandler Resolve(
            ImportService importService,
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
            string handlerClass)
        {
            // Make services that depend on snapshot config entries
            EventTypeAvroHandler avroHandler = EventTypeAvroHandlerUnsupported.INSTANCE;
            if (avroSettings.IsEnableAvro) {
                try {
                    avroHandler = TypeHelper.Instantiate<EventTypeAvroHandler>(
                        handlerClass,
                        importService.TypeResolver);
                }
                catch (Exception t) {
                    Log.Warn(
                        "Avro provider {} not instantiated, not enabling Avro support: {}",
                        handlerClass,
                        t.Message);
                }

                try {
                    avroHandler.Init(avroSettings, importService);
                }
                catch (Exception t) {
                    throw new ConfigurationException("Failed to initialize Esper-Avro: " + t.Message, t);
                }
            }

            return avroHandler;
        }
    }
} // end of namespace