///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.events.avro;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.core.support
{
    public class SupportEventAdapterService
    {
#if false
        private static EventAdapterService _eventAdapterService;

        static SupportEventAdapterService()
        {
            _eventAdapterService = Allocate(
                new DefaultLockManager(timeout => new MonitorLock(timeout)),
                new ClassLoaderProviderDefault(
                    new ClassLoaderDefault(
                        new DefaultResourceManager(true, null)
                    )));
        }

        public static void Reset(
            ILockManager lockManager,
            ClassLoaderProvider classLoaderProvider)
        {
            _eventAdapterService = Allocate(lockManager, classLoaderProvider);
        }

        public static EventAdapterService GetService(IContainer container)
        {
            return _eventAdapterService;
        }
#endif

        public static EventAdapterService Allocate(
            IContainer container,
            ClassLoaderProvider classLoaderProvider)
        {
            EventAdapterAvroHandler avroHandler = EventAdapterAvroHandlerUnsupported.INSTANCE;
            try
            {
                avroHandler = TypeHelper.Instantiate<EventAdapterAvroHandler>(
                    EventAdapterAvroHandlerConstants.HANDLER_IMPL, ClassForNameProviderDefault.INSTANCE);
            }
            catch
            {
            }

            return new EventAdapterServiceImpl(
                container,
                new EventTypeIdGeneratorImpl(), 5, avroHandler, 
                SupportEngineImportServiceFactory.Make(classLoaderProvider));
        }
    }
} // end of namespace
