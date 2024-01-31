///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.client.plugin
{
    /// <summary>
    ///     Context for plugin initialization.
    /// </summary>
    public class PluginLoaderInitContext
    {
        /// <summary>
        ///     Initialization context for use with the adapter loader.
        /// </summary>
        /// <param name="name">is the loader name</param>
        /// <param name="properties">is a set of properties from the configuration</param>
        /// <param name="runtime">is the SPI of the runtime itself for sending events to</param>
        /// <param name="configXml">config xml</param>
        public PluginLoaderInitContext(
            string name,
            Properties properties,
            string configXml,
            EPRuntime runtime)
        {
            Name = name;
            Properties = properties;
            ConfigXml = configXml;
            Runtime = runtime;
        }

        /// <summary>
        ///     Returns plugin name.
        /// </summary>
        /// <returns>plugin name</returns>
        public string Name { get; }

        /// <summary>
        ///     Returns plugin properties.
        /// </summary>
        /// <returns>plugin properties</returns>
        public Properties Properties { get; }

        /// <summary>
        ///     Returns plugin configuration XML, if any.
        /// </summary>
        /// <returns>configuration XML</returns>
        public string ConfigXml { get; }

        /// <summary>
        ///     Returns the runtime loading the plugin.
        /// </summary>
        /// <returns>runtime</returns>
        public EPRuntime Runtime { get; }
    }
} // end of namespace