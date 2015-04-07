///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Holds configuration for a plugin such as an input/output
    /// adapter loader.
    /// </summary>
    [Serializable]
    public class ConfigurationPluginLoader
    {
        /// <summary>
        /// Gets or sets the loader class name.
        /// </summary>
        /// <value>The name of the class.</value>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the config properties.
        /// </summary>
        /// <value>The config properties.</value>
        public Properties ConfigProperties { get; set; }

        /// <summary>
        /// Gets or sets the configuration XML.
        /// </summary>
        /// <value>The configuration XML.</value>
        public string ConfigurationXML { get; set; }

        /// <summary>
        /// Gets or sets the name of the loader.
        /// </summary>
        /// <value>The name of the loader.</value>
        public string LoaderName { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("MaskTypeName: {0}, ConfigProperties: {1}, LoaderName: {2}", TypeName, ConfigProperties, LoaderName);
        }
    }
}
