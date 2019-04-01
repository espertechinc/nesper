///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.runtime;

namespace com.espertech.esper.common.@internal.settings
{
    /// <summary>
    ///     Service for runtime-level settings around threading and concurrency.
    /// </summary>
    public class RuntimeSettingsService
    {
        public RuntimeSettingsService(
            ConfigurationCommon configurationCommon, ConfigurationRuntime configurationRuntime)
        {
            ConfigurationCommon = configurationCommon;
            ConfigurationRuntime = configurationRuntime;
        }

        public ConfigurationRuntime ConfigurationRuntime { get; }

        public ConfigurationCommon ConfigurationCommon { get; }
    }
} // end of namespace