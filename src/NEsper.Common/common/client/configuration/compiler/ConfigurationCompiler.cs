///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.compiler
{
    /// <summary>
    ///     Contains settings that apply to the compiler only (and that do not apply at runtime).
    /// </summary>
    [Serializable]
    public class ConfigurationCompiler
    {
        /// <summary>
        ///     Constructs an empty configuration.
        /// </summary>
        public ConfigurationCompiler()
        {
            Reset();
        }

        /// <summary>
        ///     Returns the list of plug-in views.
        /// </summary>
        /// <value>plug-in views</value>
        public IList<ConfigurationCompilerPlugInView> PlugInViews { get; private set; }

        /// <summary>
        ///     Returns the list of plug-in virtual data windows.
        /// </summary>
        /// <value>plug-in virtual data windows</value>
        public IList<ConfigurationCompilerPlugInVirtualDataWindow> PlugInVirtualDataWindows { get; private set; }

        /// <summary>
        ///     Returns the list of plug-in aggregation functions.
        /// </summary>
        /// <value>plug-in aggregation functions</value>
        public IList<ConfigurationCompilerPlugInAggregationFunction> PlugInAggregationFunctions { get; private set; }

        /// <summary>
        ///     Returns the list of plug-in aggregation multi-functions.
        /// </summary>
        /// <value>plug-in aggregation multi-functions</value>
        public IList<ConfigurationCompilerPlugInAggregationMultiFunction> PlugInAggregationMultiFunctions { get; private set; }

        /// <summary>
        ///     Returns the list of plug-in single-row functions.
        /// </summary>
        /// <value>plug-in single-row functions</value>
        public IList<ConfigurationCompilerPlugInSingleRowFunction> PlugInSingleRowFunctions { get; private set; }

        /// <summary>
        ///     Returns the list of plug-in pattern objects.
        /// </summary>
        /// <value>plug-in pattern objects</value>
        public IList<ConfigurationCompilerPlugInPatternObject> PlugInPatternObjects { get; private set; }


        /// <summary>
        ///     Returns the list of configured plug-in date-time-methods.
        /// </summary>
        public IList<ConfigurationCompilerPlugInDateTimeMethod> PlugInDateTimeMethods { get; private set; }

        /// <summary>
        ///     Returns the list of configured plug-in enum-methods.
        /// </summary>
        public IList<ConfigurationCompilerPlugInEnumMethod> PlugInEnumMethods { get; private set; }

        /// <summary>
        ///     Returns the compiler serializer-deserializer configuration.
        /// </summary>
        public ConfigurationCompilerSerde Serde { get; private set; }

        /// <summary>
        ///     Returns code generation settings
        /// </summary>
        /// <value>code generation settings</value>
        public ConfigurationCompilerByteCode ByteCode { get; set; }

        /// <summary>
        ///     Returns settings applicable to streams (insert and remove, insert only or remove only) selected for a statement.
        /// </summary>
        /// <value>stream selection defaults</value>
        public ConfigurationCompilerStreamSelection StreamSelection { get; private set; }

        /// <summary>
        ///     Returns view resources defaults.
        /// </summary>
        /// <value>view resources defaults</value>
        public ConfigurationCompilerViewResources ViewResources { get; private set; }

        /// <summary>
        ///     Returns logging settings applicable to compiler.
        /// </summary>
        /// <value>logging settings</value>
        public ConfigurationCompilerLogging Logging { get; private set; }

        /// <summary>
        ///     Returns the expression-related settings for compiler.
        /// </summary>
        /// <value>expression-related settings</value>
        public ConfigurationCompilerExpression Expression { get; private set; }

        /// <summary>
        ///     Returns statement execution-related settings, settings that
        ///     influence event/schedule to statement processing.
        /// </summary>
        /// <value>execution settings</value>
        public ConfigurationCompilerExecution Execution { get; set; }

        /// <summary>
        ///     Returns script settings.
        /// </summary>
        /// <value>script settings</value>
        public ConfigurationCompilerScripts Scripts { get; set; }

        /// <summary>
        ///     Returns the language-related settings.
        /// </summary>
        /// <value>language-related settings</value>
        public ConfigurationCompilerLanguage Language { get; set; }

        /// <summary>
        ///     Adds a plug-in aggregation function given a EPL function name and an aggregation forge class name.
        ///     <para />
        ///     The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationForgeClassName">
        ///     is the fully-qualified class name of the class implementing the aggregation
        ///     function forge interface
        /// </param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionForge(
            string functionName,
            string aggregationForgeClassName)
        {
            var entry = new ConfigurationCompilerPlugInAggregationFunction();
            entry.Name = functionName;
            entry.ForgeClassName = aggregationForgeClassName;
            PlugInAggregationFunctions.Add(entry);
        }

        public void AddPlugInAggregationFunctionForge(
            string functionName,
            Type aggregationForgeClass)
        {
            AddPlugInAggregationFunctionForge(functionName, aggregationForgeClass.FullName);
        }

        /// <summary>
        ///     Adds a plug-in aggregation multi-function.
        /// </summary>
        /// <param name="config">the configuration</param>
        public void AddPlugInAggregationMultiFunction(ConfigurationCompilerPlugInAggregationMultiFunction config)
        {
            PlugInAggregationMultiFunctions.Add(config);
        }

        /// <summary>
        ///     Add a plug-in single-row function
        /// </summary>
        /// <param name="singleRowFunction">configuration</param>
        public void AddPlugInSingleRowFunction(ConfigurationCompilerPlugInSingleRowFunction singleRowFunction)
        {
            PlugInSingleRowFunctions.Add(singleRowFunction);
        }

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name and a method name.
        ///     <para />
        ///     The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the single-row function</throws>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName)
        {
            AddPlugInSingleRowFunction(
                functionName,
                className,
                methodName,
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName)
        {
            AddPlugInSingleRowFunction(
                functionName,
                clazz.FullName,
                methodName,
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED);
        }

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache
        ///     behavior.
        ///     <para />
        ///     The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <param name="valueCache">set the behavior for caching the return value when constant parameters are provided</param>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache)
        {
            AddPlugInSingleRowFunction(
                functionName,
                className,
                methodName,
                valueCache,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache)
        {
            AddPlugInSingleRowFunction(
                functionName,
                clazz.FullName,
                methodName,
                valueCache,
                ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum.ENABLED);
        }

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache
        ///     behavior.
        ///     <para />
        ///     The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <param name="filterOptimizable">
        ///     whether the single-row function, when used in filters, may be subject to reverse index
        ///     lookup based on the function result
        /// </param>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(
                functionName,
                className,
                methodName,
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                filterOptimizable);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(
                functionName,
                clazz.FullName,
                methodName,
                ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum.DISABLED,
                filterOptimizable);
        }

        /// <summary>
        ///     Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, valueCache, filterOptimizable, false);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(functionName, clazz.FullName, methodName, valueCache, filterOptimizable, false);
        }

        /// <summary>
        ///     Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <param name="rethrowExceptions">whether to rethrow exceptions</param>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions)
        {
            var entry = new ConfigurationCompilerPlugInSingleRowFunction();
            entry.FunctionClassName = className;
            entry.FunctionMethodName = methodName;
            entry.Name = functionName;
            entry.ValueCache = valueCache;
            entry.FilterOptimizable = filterOptimizable;
            entry.RethrowExceptions = rethrowExceptions;
            AddPlugInSingleRowFunction(entry);
        }

        /// <summary>
        ///     Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="clazz">the class containing the function</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <param name="rethrowExceptions">whether to rethrow exceptions</param>
        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ConfigurationCompilerPlugInSingleRowFunction.ValueCacheEnum valueCache,
            ConfigurationCompilerPlugInSingleRowFunction.FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions)
        {
            AddPlugInSingleRowFunction(
                functionName,
                clazz.FullName,
                methodName,
                valueCache,
                filterOptimizable,
                rethrowExceptions);
        }

        /// <summary>
        ///     Add a view for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the view should be available under</param>
        /// <param name="name">is the name of the view</param>
        /// <param name="viewForgeClass">is the view forge class to use</param>
        public void AddPlugInView(
            string @namespace,
            string name,
            string viewForgeClass)
        {
            var configurationPlugInView = new ConfigurationCompilerPlugInView();
            configurationPlugInView.Namespace = @namespace;
            configurationPlugInView.Name = name;
            configurationPlugInView.ForgeClassName = viewForgeClass;
            PlugInViews.Add(configurationPlugInView);
        }

        public void AddPlugInView(
            string @namespace,
            string name,
            Type viewForgeClass)
        {
            AddPlugInView(@namespace, name, viewForgeClass.FullName);
        }

        /// <summary>
        ///     Add a virtual data window for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="forgeClass">is the view forge class to use</param>
        public void AddPlugInVirtualDataWindow(
            string @namespace,
            string name,
            string forgeClass)
        {
            AddPlugInVirtualDataWindow(@namespace, name, forgeClass, null);
        }

        public void AddPlugInVirtualDataWindow(
            string @namespace,
            string name,
            Type forgeClass)
        {
            AddPlugInVirtualDataWindow(@namespace, name, forgeClass.FullName, null);
        }

        /// <summary>
        ///     Add a virtual data window for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="forgeClass">is the view forge class to use</param>
        /// <param name="customConfigurationObject">additional configuration to be passed along</param>
        public void AddPlugInVirtualDataWindow(
            string @namespace,
            string name,
            string forgeClass,
            object customConfigurationObject)
        {
            var configurationPlugInVirtualDataWindow = new ConfigurationCompilerPlugInVirtualDataWindow();
            configurationPlugInVirtualDataWindow.Namespace = @namespace;
            configurationPlugInVirtualDataWindow.Name = name;
            configurationPlugInVirtualDataWindow.ForgeClassName = forgeClass;
            configurationPlugInVirtualDataWindow.Config = customConfigurationObject;
            PlugInVirtualDataWindows.Add(configurationPlugInVirtualDataWindow);
        }

        public void AddPlugInVirtualDataWindow(
            string @namespace,
            string name,
            Type forgeClass,
            object customConfigurationObject)
        {
            AddPlugInVirtualDataWindow(@namespace, name, forgeClass.FullName, customConfigurationObject);
        }


        /// <summary>
        ///     Add a pattern event observer for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the observer should be available under</param>
        /// <param name="name">is the name of the observer</param>
        /// <param name="observerForgeClass">is the observer forge class to use</param>
        public void AddPlugInPatternObserver(
            string @namespace,
            string name,
            string observerForgeClass)
        {
            var entry = new ConfigurationCompilerPlugInPatternObject();
            entry.Namespace = @namespace;
            entry.Name = name;
            entry.ForgeClassName = observerForgeClass;
            entry.PatternObjectType = PatternObjectType.OBSERVER;
            PlugInPatternObjects.Add(entry);
        }

        /// <summary>
        ///     Add a pattern guard for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the guard should be available under</param>
        /// <param name="name">is the name of the guard</param>
        /// <param name="guardForgeClass">is the guard forge class to use</param>
        public void AddPlugInPatternGuard(
            string @namespace,
            string name,
            string guardForgeClass)
        {
            var entry = new ConfigurationCompilerPlugInPatternObject();
            entry.Namespace = @namespace;
            entry.Name = name;
            entry.ForgeClassName = guardForgeClass;
            entry.PatternObjectType = PatternObjectType.GUARD;
            PlugInPatternObjects.Add(entry);
        }

        public void AddPlugInPatternGuard(
            string @namespace,
            string name,
            Type guardForgeClass)
        {
            AddPlugInPatternGuard(@namespace, name, guardForgeClass.FullName);
        }

        /// <summary>
        ///     Add a plug-in date-time method
        /// </summary>
        /// <param name="dateTimeMethodName">method name</param>
        /// <param name="dateTimeMethodForgeFactoryClassName">fully-qualified forge class name</param>
        public void AddPlugInDateTimeMethod(
            string dateTimeMethodName,
            string dateTimeMethodForgeFactoryClassName)
        {
            PlugInDateTimeMethods.Add(
                new ConfigurationCompilerPlugInDateTimeMethod(
                    dateTimeMethodName,
                    dateTimeMethodForgeFactoryClassName));
        }

        /// <summary>
        ///     Add a plug-in date-time method
        /// </summary>
        /// <param name="dateTimeMethodName">method name</param>
        /// <param name="dateTimeMethodForgeFactoryClass">class</param>
        public void AddPlugInDateTimeMethod(
            string dateTimeMethodName,
            Type dateTimeMethodForgeFactoryClass)
        {
            PlugInDateTimeMethods.Add(
                new ConfigurationCompilerPlugInDateTimeMethod(
                    dateTimeMethodName,
                    dateTimeMethodForgeFactoryClass.FullName));
        }

        /// <summary>
        ///     Add a plug-in enum method
        /// </summary>
        /// <param name="enumMethodName">method name</param>
        /// <param name="enumMethodForgeFactoryClassName">fully-qualified forge class name</param>
        public void AddPlugInEnumMethod(
            string enumMethodName,
            string enumMethodForgeFactoryClassName)
        {
            PlugInEnumMethods.Add(
                new ConfigurationCompilerPlugInEnumMethod(
                    enumMethodName,
                    enumMethodForgeFactoryClassName));
        }

        /// <summary>
        ///     Add a plug-in enum method
        /// </summary>
        /// <param name="enumMethodName">method name</param>
        /// <param name="enumMethodForgeFactoryClass">class</param>
        public void AddPlugInEnumMethod(
            string enumMethodName,
            Type enumMethodForgeFactoryClass)
        {
            PlugInEnumMethods.Add(new ConfigurationCompilerPlugInEnumMethod(enumMethodName, enumMethodForgeFactoryClass.FullName));
        }

        /// <summary>
        ///     Reset to an empty configuration.
        /// </summary>
        protected void Reset()
        {
            PlugInViews = new List<ConfigurationCompilerPlugInView>();
            PlugInVirtualDataWindows = new List<ConfigurationCompilerPlugInVirtualDataWindow>();
            PlugInAggregationFunctions = new List<ConfigurationCompilerPlugInAggregationFunction>();
            PlugInAggregationMultiFunctions = new List<ConfigurationCompilerPlugInAggregationMultiFunction>();
            PlugInSingleRowFunctions = new List<ConfigurationCompilerPlugInSingleRowFunction>();
            PlugInDateTimeMethods = new List<ConfigurationCompilerPlugInDateTimeMethod>();
            PlugInEnumMethods = new List<ConfigurationCompilerPlugInEnumMethod>();
            PlugInPatternObjects = new List<ConfigurationCompilerPlugInPatternObject>();
            ByteCode = new ConfigurationCompilerByteCode();
            StreamSelection = new ConfigurationCompilerStreamSelection();
            ViewResources = new ConfigurationCompilerViewResources();
            Logging = new ConfigurationCompilerLogging();
            Expression = new ConfigurationCompilerExpression();
            Execution = new ConfigurationCompilerExecution();
            Scripts = new ConfigurationCompilerScripts();
            Language = new ConfigurationCompilerLanguage();
            Serde = new ConfigurationCompilerSerde();
        }
    }
} // end of namespace