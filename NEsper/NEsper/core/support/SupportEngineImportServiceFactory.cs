///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.container;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.core.support
{
    public class SupportEngineImportServiceFactory
    {
        public static EngineImportServiceImpl Make(IContainer container)
        {
            return Make(container.Resolve<ClassLoaderProvider>());
        }

        public static EngineImportServiceImpl Make(ClassLoaderProvider classLoaderProvider)
        {
            return new EngineImportServiceImpl(
                true, true, true, false, null,
                TimeZoneInfo.Local,
                TimeAbacusMilliseconds.INSTANCE,
                ConfigurationEngineDefaults.ThreadingProfile.NORMAL, null,
                AggregationFactoryFactoryDefault.INSTANCE,
                false, "default", null,
                classLoaderProvider);
        }
    }
} // end of namespace
