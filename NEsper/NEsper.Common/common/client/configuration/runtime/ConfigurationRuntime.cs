///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.configuration.runtime
{
    /// <summary>
    ///     Contains settings that apply to the runtime only (and that do not apply to the compiler).
    /// </summary>
    [Serializable]
    public class ConfigurationRuntime
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public ConfigurationRuntime()
        {
            Reset();
        }

        /// <summary>
        ///     Returns the class name of the services context factory class to use.
        /// </summary>
        /// <returns>class name</returns>
        public string EPServicesContextFactoryClassName => EpServicesContextFactoryClassName;

        /// <summary>
        ///     Returns the metrics reporting configuration.
        /// </summary>
        /// <returns>metrics reporting config</returns>
        public ConfigurationRuntimeMetricsReporting MetricsReporting { get; set; }

        /// <summary>
        ///     Returns the exception handling configuration.
        /// </summary>
        /// <returns>exception handling configuration</returns>
        public ConfigurationRuntimeExceptionHandling ExceptionHandling { get; set; }

        /// <summary>
        ///     Returns the condition handling configuration.
        /// </summary>
        /// <returns>condition handling configuration</returns>
        public ConfigurationRuntimeConditionHandling ConditionHandling { get; set; }

        /// <summary>
        ///     Returns threading settings.
        /// </summary>
        /// <returns>threading settings object</returns>
        public ConfigurationRuntimeThreading Threading { get; set; }

        /// <summary>
        ///     Return match-recognize settings.
        /// </summary>
        /// <returns>match-recognize settings</returns>
        public ConfigurationRuntimeMatchRecognize MatchRecognize { get; set; }

        /// <summary>
        ///     Return pattern settings.
        /// </summary>
        /// <returns>pattern settings</returns>
        public ConfigurationRuntimePatterns Patterns { get; set; }

        /// <summary>
        ///     Returns defaults applicable to variables.
        /// </summary>
        /// <returns>variable defaults</returns>
        public ConfigurationRuntimeVariables Variables { get; set; }

        /// <summary>
        ///     Returns logging settings applicable to runtime.
        /// </summary>
        /// <returns>logging settings</returns>
        public ConfigurationRuntimeLogging Logging { get; set; }

        /// <summary>
        ///     Returns the time source configuration.
        /// </summary>
        /// <returns>time source enum</returns>
        public ConfigurationRuntimeTimeSource TimeSource { get; set; }

        /// <summary>
        ///     Returns the expression-related settings for common.
        /// </summary>
        /// <returns>expression-related settings</returns>
        public ConfigurationRuntimeExpression Expression { get; set; }

        /// <summary>
        ///     Returns statement execution-related settings, settings that
        ///     influence event/schedule to statement processing.
        /// </summary>
        /// <returns>execution settings</returns>
        public ConfigurationRuntimeExecution Execution { get; set; }

        /// <summary>
        ///     Returns the plug-in loaders.
        /// </summary>
        /// <value>plug-in loaders</value>
        public IList<ConfigurationRuntimePluginLoader> PluginLoaders { get; set; }

        /// <summary>
        ///     Sets the class name of the services context factory class to use.
        /// </summary>
        /// <value>service context factory class name</value>
        public string EpServicesContextFactoryClassName { get; set; }

        /// <summary>
        ///     Add a plugin loader (f.e. an input/output adapter loader).
        ///     <p>The class is expected to implement the PluginLoader interface.</p>.
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        /// <param name="configuration">is loader cofiguration entries</param>
        public void AddPluginLoader(
            string loaderName,
            string className,
            Properties configuration)
        {
            AddPluginLoader(loaderName, className, configuration, null);
        }

        public void AddPluginLoader(
            string loaderName,
            Type clazz,
            Properties configuration)
        {
            AddPluginLoader(loaderName, clazz.FullName, configuration, null);
        }

        /// <summary>
        ///     Add a plugin loader (f.e. an input/output adapter loader) without any additional loader configuration
        ///     <p>The class is expected to implement the PluginLoader interface.</p>.
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        public void AddPluginLoader(
            string loaderName,
            string className)
        {
            AddPluginLoader(loaderName, className, null, null);
        }

        /// <summary>
        ///     Add a plugin loader (f.e. an input/output adapter loader).
        ///     <p>The class is expected to implement the PluginLoader interface.</p>.
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        /// <param name="configuration">is loader cofiguration entries</param>
        /// <param name="configurationXML">config xml if any</param>
        public void AddPluginLoader(
            string loaderName,
            string className,
            Properties configuration,
            string configurationXML)
        {
            var pluginLoader = new ConfigurationRuntimePluginLoader();
            pluginLoader.LoaderName = loaderName;
            pluginLoader.ClassName = className;
            pluginLoader.ConfigProperties = configuration;
            pluginLoader.ConfigurationXML = configurationXML;
            PluginLoaders.Add(pluginLoader);
        }

        /// <summary>
        ///     Reset to an empty configuration.
        /// </summary>
        protected void Reset()
        {
            PluginLoaders = new List<ConfigurationRuntimePluginLoader>();
            MetricsReporting = new ConfigurationRuntimeMetricsReporting();
            ExceptionHandling = new ConfigurationRuntimeExceptionHandling();
            ConditionHandling = new ConfigurationRuntimeConditionHandling();
            Threading = new ConfigurationRuntimeThreading();
            MatchRecognize = new ConfigurationRuntimeMatchRecognize();
            Patterns = new ConfigurationRuntimePatterns();
            Variables = new ConfigurationRuntimeVariables();
            Logging = new ConfigurationRuntimeLogging();
            TimeSource = new ConfigurationRuntimeTimeSource();
            Expression = new ConfigurationRuntimeExpression();
            Execution = new ConfigurationRuntimeExecution();
        }
    }
} // end of namespace