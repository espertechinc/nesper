///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Xml;

namespace com.espertech.esper.client
{
    public interface IConfigurationParser
    {
        /// <summary>
        /// Parse the W3C DOM document.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="doc">to parse</param>
        /// <exception cref="EPException">to indicate parse errors</exception>
        void DoConfigure(Configuration configuration, XmlDocument doc);

        /// <summary>
        /// Parse the W3C DOM element.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="rootElement">The root element.</param>
        /// <exception cref="EPException">to indicate parse errors</exception>
        void DoConfigure(Configuration configuration, XmlElement rootElement);

        /// <summary>
        /// Use the configuration specified in the given input stream.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="stream">The stream.</param>
        /// <param name="resourceName">The name to use in warning/error messages</param>
        /// <throws>  com.espertech.esper.client.EPException </throws>
        void DoConfigure(Configuration configuration, Stream stream, String resourceName);
    }
}