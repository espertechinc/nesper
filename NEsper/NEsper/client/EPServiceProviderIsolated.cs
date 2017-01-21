///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Isolated service provider for controlling event visibility and scheduling on a
    /// statement level.
    /// </summary>
    public interface EPServiceProviderIsolated : IDisposable
    {
        /// <summary>
        /// Returns a class instance of EPRuntime.
        /// </summary>
        /// <returns>
        /// an instance of EPRuntime
        /// </returns>
        EPRuntimeIsolated EPRuntime { get; }

        /// <summary>
        /// Returns a class instance of EPAdministrator.
        /// </summary>
        /// <returns>
        /// an instance of EPAdministrator
        /// </returns>
        EPAdministratorIsolated EPAdministrator { get; }

        /// <summary>
        /// Name of isolated service.
        /// </summary>
        /// <returns>
        /// isolated service name
        /// </returns>
        string Name { get; }
    }
}
