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
    ///     Configuration object for plug-in event representations.
    /// </summary>
    [Serializable]
    public class ConfigurationPlugInEventRepresentation
    {
        private string _eventRepresentationTypeName;
        private object _initializer;

        /// <summary>
        ///     Gets or sets the class name of the class providing the pluggable event representation.
        /// </summary>
        /// <value>The name of the event representation type.</value>
        /// <returns>class name of class implementing <see cref="com.espertech.esper.plugin.PlugInEventRepresentation" /></returns>
        public string EventRepresentationTypeName {
            get => _eventRepresentationTypeName;
            set => _eventRepresentationTypeName = value;
        }

        /// <summary>
        ///     Gets or sets the optional initialization or configuration information for the plug-in event
        ///     representation.
        /// </summary>
        /// <value>The initializer.</value>
        /// <returns>
        ///     any configuration object specific to the event representation, or a XML string
        ///     if supplied via configuration XML file, or null if none supplied
        /// </returns>
        public object Initializer {
            get => _initializer;
            set => _initializer = value;
        }
    }
}