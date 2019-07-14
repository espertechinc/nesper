///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.thread;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public interface EPRuntimeSPI : EPRuntime
    {
        void SetConfiguration(Configuration configuration);

        void PostInitialize();

        void Initialize(long? currentTime);

        IContainer Container { get; }

        EPServicesContext ServicesContext { get; }

        AtomicBoolean ServiceStatusProvider { get; }

        EPEventServiceSPI EventServiceSPI { get; }

        ThreadingService ThreadingService { get; }
    }
} // end of namespace