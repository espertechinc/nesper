///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.avro;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.support
{
    public class SupportEventAdapterService
    {
        private static EventAdapterService _eventAdapterService;

        static SupportEventAdapterService()
        {
            _eventAdapterService = Allocate();
        }

        public static void Reset()
        {
            _eventAdapterService = Allocate();
        }

        public static EventAdapterService Service
        {
            get { return _eventAdapterService; }
        }

        private static EventAdapterService Allocate()
        {
            EventAdapterAvroHandler avroHandler = EventAdapterAvroHandlerUnsupported.INSTANCE;
            try
            {
                avroHandler =
                    TypeHelper.Instantiate<EventAdapterAvroHandler>(
                        EventAdapterAvroHandlerConstants.HANDLER_IMPL, ClassForNameProviderDefault.INSTANCE);
            }
            catch
            {
            }

            return new EventAdapterServiceImpl(
                new EventTypeIdGeneratorImpl(), 5, avroHandler, SupportEngineImportServiceFactory.Make());
        }
    }
} // end of namespace
