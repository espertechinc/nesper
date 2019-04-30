///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
            finally {
                try {
                    stream.Close();
                }
                catch (IOException ioe) {
                    Log.Warn("could not close input stream for: " + resourceName, ioe);
                }
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
                if (nodeName.Equals("common")) {
                    ConfigurationCommonParser.DoConfigure(configuration.Common, element);
                }
                else if (nodeName.Equals("compiler")) {
                    ConfigurationCompilerParser.DoConfigure(configuration.Compiler, element);
                }
                else if (nodeName.Equals("runtime")) {
                    ConfigurationRuntimeParser.DoConfigure(configuration.Runtime, element);
                }
                else if (nodeName.Equals("event-type") ||
                         nodeName.Equals("auto-import") ||
                         nodeName.Equals("auto-import-annotations") ||
                         nodeName.Equals("method-reference") ||
                         nodeName.Equals("database-reference") ||
                         nodeName.Equals("plugin-view") ||
                         nodeName.Equals("plugin-virtualdw") ||
                         nodeName.Equals("plugin-aggregation-function") ||
                         nodeName.Equals("plugin-aggregation-multifunction") ||
                         nodeName.Equals("plugin-singlerow-function") ||
                         nodeName.Equals("plugin-pattern-guard") ||
                         nodeName.Equals("plugin-pattern-observer") ||
                         nodeName.Equals("variable") ||
                         nodeName.Equals("plugin-loader") ||
                         nodeName.Equals("engine-settings") ||
                         nodeName.Equals("variant-stream")) {
                    Log.Warn(
                        "The configuration file appears outdated as it has element '" + nodeName +
                        "' among top-level elements. Please convert to the newest schema using the online converter.");
                }
            }
        }

        private readonly static ILog Log = LogManager.GetLogger(typeof(ConfigurationParser));
    }
} // end of namespace