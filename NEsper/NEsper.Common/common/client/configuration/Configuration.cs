///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.compiler;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

namespace com.espertech.esper.common.client.configuration
{
    /// <summary>
    /// An instance of <tt>Configuration</tt> allows the application
    /// to specify properties to be used when compiling and when getting a runtime.
    /// The <tt>Configuration</tt> is meant
    /// only as an initialization-time object.
    /// The format of an Esper XML configuration file is defined in
    /// <tt>esper-configuration-(version).xsd</tt>.
    /// </summary>
    public class Configuration
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default name of the configuration file.
        /// </summary>
        protected const string ESPER_DEFAULT_CONFIG = "esper.cfg.xml";

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        private IContainer Container { get; set; }

        /// <summary>
        /// Constructs an empty configuration. The auto import values are set to defaults.
        /// </summary>
        public Configuration()
        {
            Container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();
            Reset();
        }

        /// <summary>
        /// Constructs an empty configuration. The auto import values are set to defaults.
        /// </summary>
        public Configuration(IContainer container)
        {
            Container = container;
            Reset();
        }

        /// <summary>
        /// Use the configuration specified in an application
        /// resource named <tt>esper.cfg.xml</tt>.
        /// </summary>
        /// <returns>Configuration initialized from the resource</returns>
        /// <throws>EPException thrown to indicate error reading configuration</throws>
        public Configuration Configure()
        {
            Configure('/' + ESPER_DEFAULT_CONFIG);
            return this;
        }

        /// <summary>
        /// Use the configuration specified in the given application
        /// resource. The format of the resource is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// <para />The resource is found via <tt>getConfigurationInputStream(resource)</tt>.
        /// That method can be overridden to implement an arbitrary lookup strategy.
        /// <para />See <tt>getResourceAsStream</tt> for information on how the resource name is resolved.
        /// </summary>
        /// <param name="resource">if the file name of the resource</param>
        /// <returns>Configuration initialized from the resource</returns>
        /// <throws>EPException thrown to indicate error reading configuration</throws>
        public Configuration Configure(string resource)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("Configuring from resource: " + resource);
            }

            Stream stream = GetConfigurationInputStream(resource);
            ConfigurationParser.DoConfigure(this, stream, resource);
            return this;
        }

        /// <summary>
        /// Get the configuration file as an <tt>InputStream</tt>. Might be overridden
        /// by subclasses to allow the configuration to be located by some arbitrary
        /// mechanism.
        /// <para />See <tt>getResourceAsStream</tt> for information on how the resource name is resolved.
        /// </summary>
        /// <param name="resource">is the resource name</param>
        /// <returns>input stream for resource</returns>
        /// <throws>EPException thrown to indicate error reading configuration</throws>
        protected internal static Stream GetConfigurationInputStream(string resource)
        {
            return GetResourceAsStream(resource);
        }

        /// <summary>
        /// Use the configuration specified by the given URL.
        /// The format of the document obtained from the URL is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="url">URL from which you wish to load the configuration</param>
        /// <returns>A configuration configured via the file</returns>
        /// <throws>EPException is thrown when the URL could not be access</throws>
        public Configuration Configure(Uri url)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("configuring from url: " + url);
            }

            try {
                ConfigurationParser.DoConfigure(this, url.OpenStream(), url.ToString());
                return this;
            }
            catch (IOException ioe) {
                throw new EPException("could not configure from URL: " + url, ioe);
            }
        }

        /// <summary>
        /// Use the configuration specified in the given application
        /// file. The format of the file is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="configFile">&lt;tt&gt;File&lt;/tt&gt; from which you wish to load the configuration</param>
        /// <returns>A configuration configured via the file</returns>
        /// <throws>EPException when the file could not be found</throws>
        public Configuration Configure(FileInfo configFile)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("configuring from file: " + configFile.Name);
            }

            Stream inputStream = null;
            try {
                inputStream = File.OpenRead(configFile.ToString());
                ConfigurationParser.DoConfigure(this, inputStream, configFile.ToString());
            }
            catch (FileNotFoundException fnfe) {
                throw new EPException("could not find file: " + configFile, fnfe);
            }
            finally {
                if (inputStream != null) {
                    try {
                        inputStream.Close();
                    }
                    catch (IOException e) {
                        Log.Debug("Error closing input stream", e);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Use the mappings and properties specified in the given XML document.
        /// The format of the file is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="document">an XML document from which you wish to load the configuration</param>
        /// <returns>A configuration configured via the &lt;tt&gt;Document&lt;/tt&gt;</returns>
        /// <throws>EPException if there is problem in accessing the document.</throws>
        public Configuration Configure(XmlDocument document)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug("configuring from XML document");
            }

            ConfigurationParser.DoConfigure(this, document);
            return this;
        }

        /// <summary>
        /// Returns an input stream from an application resource in the classpath.
        /// <para />The method first removes the '/' character from the resource name if
        /// the first character is '/'.
        /// <para />The lookup order is as follows:
        /// <para />If a thread context class loader exists, use <tt>Thread.currentThread().getResourceAsStream</tt>to obtain an InputStream.
        /// <para />If no input stream was returned, use the <tt>Configuration.class.getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// <para />If no input stream was returned, use the <tt>Configuration.class.getClassLoader().getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// <para />If no input stream was returned, throw an Exception.
        /// </summary>
        /// <param name="resource">to get input stream for</param>
        /// <returns>input stream for resource</returns>
        protected internal static Stream GetResourceAsStream(string resource)
        {
            string stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;

            Stream stream = null;
            ClassLoader classLoader = Thread.CurrentThread().ContextClassLoader;
            if (classLoader != null) {
                stream = classLoader.GetResourceAsStream(stripped);
            }

            if (stream == null) {
                stream = typeof(Configuration).GetResourceAsStream(resource);
            }

            if (stream == null) {
                stream = typeof(Configuration).ClassLoader.GetResourceAsStream(stripped);
            }

            if (stream == null) {
                throw new EPException(resource + " not found");
            }

            return stream;
        }

        /// <summary>
        /// Returns the common section of the configuration.
        /// <para />The common section is for use by both the compiler and the runtime.
        /// </summary>
        /// <returns>common configuration</returns>
        public ConfigurationCommon Common { get; set; }

        /// <summary>
        /// Returns the compiler section of the configuration.
        /// <para />The compiler section is for use by the compiler. The runtime ignores this part of the configuration object.
        /// </summary>
        /// <returns>compiler configuration</returns>
        public ConfigurationCompiler Compiler { get; set; }

        /// <summary>
        /// Returns the runtime section of the configuration.
        /// <para />The runtime section is for use by the runtime. The compiler ignores this part of the configuration object.
        /// </summary>
        /// <returns>runtime configuration</returns>
        public ConfigurationRuntime Runtime { get; set; }

        /// <summary>
        /// Reset to an empty configuration.
        /// </summary>
        protected void Reset()
        {
            Common = new ConfigurationCommon();
            Compiler = new ConfigurationCompiler();
            Runtime = new ConfigurationRuntime();
        }
    }
} // end of namespace