///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    /// <summary>
    ///     Interface for a factory class to provide services in a services context for an runtime instance.
    /// </summary>
    public interface EPServicesContextFactory
    {
        /// <summary>
        ///     Factory method for a new set of runtime services.
        /// </summary>
        /// <param name="epRuntime">is the runtime instance</param>
        /// <param name="configurationSnapshot">is a snapshot of configs at the time of runtime creation</param>
        /// <returns>services context</returns>
        EPServicesContext CreateServicesContext(
            EPRuntimeSPI epRuntime,
            Configuration configurationSnapshot,
            EPRuntimeOptions options);

        EPEventServiceImpl CreateEPRuntime(
            EPServicesContext services,
            AtomicBoolean serviceStatusProvider);
    }
} // end of namespace