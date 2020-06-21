///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.xml;

namespace com.espertech.esper.common.@internal.util
{
    /// <summary>
    /// Parser for configuration XML.
    /// </summary>
    public class ConfigurationParser
    {
        /// <summary>
        /// Use the configuration specified in the given input stream.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="stream">Inputstream to be read from</param>
        /// <param name="resourceName">The name to use in warning/error messages</param>
        /// <throws>EPException is thrown when the configuration could not be parsed</throws>
        public static void DoConfigure(
            Configuration configuration,
            Stream stream,
            string resourceName)
        {
            XmlDocument document = GetDocument(stream, resourceName);
            DoConfigure(configuration, document);
        }

        public static XmlDocument GetDocument(
            Stream stream,
            string resourceName)
        {
            XmlDocument document;

            try {
                document = new XmlDocument();
                document.Load(stream);
            }
            catch (XmlException ex) {
                throw new EPException("Could not parse configuration: " + resourceName, ex);
            }
            catch (IOException ex) {
                throw new EPException("Could not read configuration: " + resourceName, ex);
            }

            return document;
        }

        /// <summary>
        /// Parse the W3C DOM document.
        /// </summary>
        /// <param name="configuration">is the configuration object to populate</param>
        /// <param name="doc">to parse</param>
        /// <throws>EPException to indicate parse errors</throws>
        public static void DoConfigure(
            Configuration configuration,
            XmlDocument doc)
        {
            var root = doc.DocumentElement;

            foreach (var element in root.ChildNodes.CreateElementEnumerable()) {
                string nodeName = element.Name;
                switch (nodeName) {
                    case "common":
                        ConfigurationCommonParser.DoConfigure(configuration.Common, element);
                        break;

                    case "compiler":
                        ConfigurationCompilerParser.DoConfigure(configuration.Compiler, element);
                        break;

                    case "runtime":
                        ConfigurationRuntimeParser.DoConfigure(configuration.Runtime, element);
                        break;

                    case "event-type":
                    case "auto-import":
                    case "auto-import-annotations":
                    case "method-reference":
                    case "database-reference":
                    case "plugin-view":
                    case "plugin-virtualdw":
                    case "plugin-aggregation-function":
                    case "plugin-aggregation-multifunction":
                    case "plugin-singlerow-function":
                    case "plugin-pattern-guard":
                    case "plugin-pattern-observer":
                    case "variable":
                    case "plugin-loader":
                    case "engine-settings":
                    case "variant-stream":
                        Log.Warn(
                            "The configuration file appears outdated as it has element '" +
                            nodeName +
                            "' among top-level elements. Please convert to the newest schema using the online converter.");
                        break;
                }
            }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationParser));
    }
} // end of namespace