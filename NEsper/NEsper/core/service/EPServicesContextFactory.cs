///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Interface for a factory class to provide services in a services context for an engine instance.
    /// </summary>
    public interface EPServicesContextFactory
    {
        /// <summary>
        /// Factory method for a new set of engine services.
        /// </summary>
        /// <param name="container">The root IoC container.</param>
        /// <param name="epServiceProvider">is the engine instance</param>
        /// <param name="configurationSnapshot">is a snapshot of configs at the time of engine creation</param>
        /// <returns>
        /// services context
        /// </returns>
        EPServicesContext CreateServicesContext(
            IContainer container,
            EPServiceProvider epServiceProvider, 
            ConfigurationInformation configurationSnapshot);    
    }
}
