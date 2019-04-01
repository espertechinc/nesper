///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.plugin
{
    /// <summary>Context for plugin initialization. </summary>
    public class PluginLoaderInitContext
    {
        /// <summary>Initialization context for use with the adapter loader. </summary>
        /// <param name="name">is the loader name</param>
        /// <param name="properties">is a set of properties from the configuration</param>
        /// <param name="epService">is the SPI of the engine itself for sending events to</param>
        /// <param name="configXml">config xml</param>
        public PluginLoaderInitContext(String name, Properties properties, String configXml, EPServiceProvider epService)
        {
            Name = name;
            Properties = properties;
            ConfigXml = configXml;
            EpServiceProvider = epService;
        }

        /// <summary>Returns plugin name. </summary>
        /// <value>plugin name</value>
        public string Name { get; private set; }

        /// <summary>Returns plugin properties. </summary>
        /// <value>plugin properties</value>
        public Properties Properties { get; private set; }

        /// <summary>Returns plugin configuration XML, if any. </summary>
        /// <value>configuration XML</value>
        public string ConfigXml { get; private set; }

        /// <summary>Returns the engine loading the plugin. </summary>
        /// <value>engine</value>
        public EPServiceProvider EpServiceProvider { get; private set; }
    }
}
