///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Marker interface for extension services that provide additional engine or 
    /// statement-level extensions, such as views backed by a write-behind store.
    /// </summary>
    public interface ExtensionServicesContext : IDisposable
    {
        /// <summary>Invoked to initialize extension services after engine services initialization. </summary>
        void Init(EPServicesContext engine, EPRuntimeSPI runtimeSPI, EPAdministratorSPI adminSPI);

        bool IsHAEnabled { get; }
    }
}
