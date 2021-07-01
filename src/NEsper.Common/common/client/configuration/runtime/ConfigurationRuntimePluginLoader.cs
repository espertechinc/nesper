///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Holds configuration for a plugin such as an input/output adapter loader.
    /// </summary>
    [Serializable]
    public class ConfigurationRuntimePluginLoader
    {
        /// <summary>
        ///     Returns the loader class name.
        /// </summary>
        /// <returns>class name of loader</returns>
        public string ClassName { get; set; }

        /// <summary>
        ///     Returns loader configuration properties.
        /// </summary>
        /// <returns>config entries</returns>
        public Properties ConfigProperties { get; set; }

        /// <summary>
        ///     Returns the loader name.
        /// </summary>
        /// <returns>loader name</returns>
        public string LoaderName { get; set; }

        /// <summary>
        ///     Returns configuration XML for the plugin.
        /// </summary>
        /// <returns>xml</returns>
        public string ConfigurationXML { get; set; }

        public override string ToString()
        {
            return "ConfigurationPluginLoader name '" +
                   LoaderName +
                   "' class '" +
                   ClassName +
                   " ' properties '" +
                   ConfigProperties +
                   "'";
        }
    }
} // end of namespace