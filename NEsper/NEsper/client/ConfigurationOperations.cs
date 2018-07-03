///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.container;

namespace com.espertech.esper.client
{
    /// <summary>
    ///     Provides configuration operations for configuration-time and runtime parameters.
    /// </summary>
    public interface ConfigurationOperations
    {
        /// <summary>
        ///     Returns an array of event types tracked or available within the engine in any order. Included are all
        ///     application-configured or EPL-created schema types
        ///     as well as dynamically-allocated stream's event types or types otherwise known to the engine as a dependeny type or
        ///     supertype to another type.
        ///     <para>
        ///         Event types that are associated to statement output may not necessarily be returned as such types,
        ///         depending on the statement, are considered anonymous.
        ///     </para>
        ///     <para>
        ///         This operation is not available for static configuration and is only available for runtime use.
        ///     </para>
        /// </summary>
        /// <value>event type array</value>
        ICollection<EventType> EventTypes { get; }

        /// <summary>
        ///     Adds a namespace that event classes reside in.
        ///     <para>
        ///         This setting allows an application to place all it's events into one or more namespaces
        ///         and then declare these namespaces via this method. The engine attempts to resolve an event 
        ///         type name to a type residing in each declared namespace.
        ///     </para>
        ///     <para>
        ///         For example, in the statement "select * from MyEvent" the engine attempts to load class
        ///         "namespace.MyEvent"
        ///         and if successful, uses that class as the event type.
        ///     </para>
        /// </summary>
        /// <param name="namespace">is the fully-qualified namespace name of the namespace that event classes reside in</param>
        void AddEventTypeAutoName(string @namespace);

        /// <summary>
        ///     Adds a plug-in aggregation multi-function.
        /// </summary>
        /// <param name="config">the configuration</param>
        /// <exception cref="ConfigurationException">is thrown to indicate a configuration problem</exception>
        void AddPlugInAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction config);

        /// <summary>
        ///     Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        ///     <para>
        ///         The same function name cannot be added twice.
        ///     </para>
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryClassName">
        ///     is the fully-qualified class name of the class implementing the aggregation
        ///     function factory interface <seealso cref="com.espertech.esper.client.hook.AggregationFunctionFactory" />
        /// </param>
        /// <exception cref="ConfigurationException">is thrown to indicate a problem adding the aggregation function</exception>
        void AddPlugInAggregationFunctionFactory(string functionName, string aggregationFactoryClassName);
        void AddPlugInAggregationFunctionFactory(string functionName, Type aggregationFactoryClass);

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache
        ///     behavior.
        ///     <para>
        ///         The same function name cannot be added twice.
        ///     </para>
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the static method provided by the class that : the single-row function</param>
        /// <param name="valueCache">set the behavior for caching the return value when constant parameters are provided</param>
        /// <exception cref="ConfigurationException">is thrown to indicate a problem adding the single-row function</exception>
        void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ValueCacheEnum valueCache);

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache
        ///     behavior.
        ///     <para>
        ///         The same function name cannot be added twice.
        ///     </para>
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the static method provided by the class that : the single-row function</param>
        /// <param name="filterOptimizable">
        ///     whether the single-row function, when used in filters, may be subject to reverse index
        ///     lookup based on the function result
        /// </param>
        /// <exception cref="ConfigurationException">is thrown to indicate a problem adding the single-row function</exception>
        void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            FilterOptimizableEnum filterOptimizable);

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name and a method name.
        ///     <para>
        ///         The same function name cannot be added twice.
        ///     </para>
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the static method provided by the class that : the single-row function</param>
        /// <exception cref="ConfigurationException">is thrown to indicate a problem adding the single-row function</exception>
        void AddPlugInSingleRowFunction(string functionName, string className, string methodName);
        void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName);

        /// <summary>
        ///     Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache
        ///     behavior.
        ///     <para>
        ///         The same function name cannot be added twice.
        ///     </para>
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the static method provided by the class that : the single-row function</param>
        /// <param name="valueCache">set the behavior for caching the return value when constant parameters are provided</param>
        /// <param name="filterOptimizable">
        ///     whether the single-row function, when used in filters, may be subject to reverse index
        ///     lookup based on the function result
        /// </param>
        /// <param name="rethrowExceptions">whether exceptions generated by the UDF are rethrown</param>
        /// <exception cref="ConfigurationException">is thrown to indicate a problem adding the single-row function</exception>
        void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions);

        /// <summary>
        /// Adds a package or class to the list of automatically-imported types.
        /// <para/>
        /// To import a single class offering a static method, simply supply the fully-qualified name of the
        /// class and use the syntax <code>classname.Methodname(...)</code>
        /// 	<para/>
        /// To import a whole package and use the <code>classname.Methodname(...)</code> syntax.
        /// </summary>
        /// <param name="importName">is a fully-qualified class name or a package name with wildcard</param>
        /// <throws>ConfigurationException if incorrect package or class names are encountered</throws>
        void AddImport(String importName);

        /// <summary>
        /// Adds the class or namespace (importName) ot the list of automatically imported types.
        /// </summary>
        /// <param name="importName">Name of the import.</param>
        /// <param name="assemblyNameOrFile">The assembly name or file.</param>
        void AddImport(String importName, String assemblyNameOrFile);

        /// <summary>
        /// Adds a class to the list of automatically-imported classes.
        /// </summary>
        void AddImport(Type importClass);

        /// <summary>
        /// Adds the import.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddImport<T>();

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="importName">Name of the import.</param>
        /// <param name="assemblyNameOrFile">The assembly name or file.</param>
        void AddAnnotationImport(String importName, String assemblyNameOrFile);

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="autoImport">The automatic import.</param>
        void AddAnnotationImport(String autoImport);

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="autoImport">The automatic import.</param>
        void AddAnnotationImport(Type autoImport);

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="importNamespace"></param>
        /// <typeparam name="T"></typeparam>
        void AddAnnotationImport<T>(bool importNamespace = false);

        /// <summary>
        /// Adds an import for the namespace associated with the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddNamespaceImport<T>();

        /// <summary>
        ///     Checks if an eventTypeName has already been registered for that name.
        /// </summary>
        /// <param name="eventTypeName">the name</param>
        /// <returns>true if already registered</returns>
        bool IsEventTypeExists(string eventTypeName);

        /// <summary>
        ///     Add an name for an event type represented by object events.
        ///     <para>
        ///         Allows a second name to be added for the same type.
        ///         Does not allow the same name to be used for different types.
        ///     </para>
        ///     <para>
        ///         Note that when adding multiple names for the same type the names represent an
        ///         alias to the same event type since event type identity for types is per type.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClassName">fully-qualified class name of the event type</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>J
        void AddEventType(string eventTypeName, string eventClassName);

        /// <summary>
        ///     Add an name for an event type represented by object events.
        ///     <para>
        ///         Allows a second name to be added for the same type.
        ///         Does not allow the same name to be used for different types.
        ///     </para>
        ///     <para>
        ///         Note that when adding multiple names for the same type the names represent an
        ///         alias to the same event type since event type identity for types is per type.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">is the event class for which to create the name</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, Type eventClass);

        /// <summary>
        ///     Add a name for an event type represented by object events,
        ///     using the simple name of the type as the name.
        ///     <para>
        ///         For example, if your class is "com.mycompany.MyEvent", then this method
        ///         adds the name "MyEvent" for the class.
        ///     </para>
        ///     <para>
        ///         Allows a second name to be added for the same type.
        ///         Does not allow the same name to be used for different types.
        ///     </para>
        /// </summary>
        /// <param name="eventClass">is the event class for which to create the name from the class simple name</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(Type eventClass);

        /// <summary>
        ///     Add a name for an event type represented by object events,
        ///     using the simple name of the type as the name.
        /// </summary>
        /// <param name="eventTypeName">is the event class for which to create the name from the class simple name</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType<T>(String eventTypeName);

        /// <summary>
        /// Adds a name for an event type represented by the type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddEventType<T>();

        /// <summary>
        ///     Add an event type that represents IDictionary events.
        ///     <para>
        ///         Allows a second name to be added for the same type.
        ///         Does not allow the same name to be used for different types.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type
        ///     (fully qualified classname) of its value in Map event instances.
        /// </param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, Properties typeMap);

        /// <summary>
        ///     Add an event type that represents Object-array (Object[]) events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, string[] propertyNames, Object[] propertyTypes);

        /// <summary>
        ///     Add an event type that represents Object-array (Object[]) events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>
        /// <param name="optionalConfiguration">object-array type configuration</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(
            string eventTypeName,
            string[] propertyNames,
            Object[] propertyTypes,
            ConfigurationEventTypeObjectArray optionalConfiguration);

        /// <summary>
        ///     Add an name for an event type that represents IDictionary events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para>
        ///         Each entry in the type mapping must contain the string property name as the key value,
        ///         and either a Type, or a further Map&lt;string, Object&gt;, or the name
        ///         of another previously-register Map event type (append [] for array of Map).
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type
        ///     (fully qualified classname) of its value in Map event instances.
        /// </param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap);

        /// <summary>
        ///     Add a name for an event type that represents IDictionary events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para>
        ///         Each entry in the type mapping must contain the string property name as the key value,
        ///         and either a Type, or a further Map&lt;string, Object&gt;, or the name
        ///         of another previously-register Map event type (append [] for array of Map).
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type
        ///     (fully qualified classname) of its value in Map event instances.
        /// </param>
        /// <param name="superTypes">is an array of event type name of further Map types that this</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap, string[] superTypes);

        /// <summary>
        ///     Add a name for an event type that represents IDictionary events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para>
        ///         Each entry in the type mapping must contain the string property name as the key value,
        ///         and either a Type, or a further Map&lt;string, Object&gt;, or the name
        ///         of another previously-register Map event type (append [] for array of Map).
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type
        ///     (fully qualified classname) of its value in Map event instances.
        /// </param>
        /// <param name="mapConfig">is the Map-event type configuration that may defined super-types, timestamp-property-name etc.</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(
            string eventTypeName,
            IDictionary<string, Object> typeMap,
            ConfigurationEventTypeMap mapConfig);

        /// <summary>
        ///     Add an name for an event type that represents org.w3c.dom.Node events.
        ///     <para>
        ///         Allows a second name to be added for the same type.
        ///         Does not allow the same name to be used for different types.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="xmlDOMEventTypeDesc">descriptor containing property and mapping information for XML-DOM events</param>
        /// <exception cref="ConfigurationException">if the name is already in used for a different type</exception>
        void AddEventType(string eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc);

        /// <summary>
        ///     Add a global variable.
        ///     <para>
        ///         Use the runtime API to set variable values or EPL statements to change variable values.
        ///     </para>
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any
        ///     value or an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned value.
        ///     For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can
        ///     be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be
        ///     Serializable.
        /// </param>
        /// <exception cref="ConfigurationException">
        ///     if the type and initialization value don't match or the variable name
        ///     is already in use
        /// </exception>
        void AddVariable(string variableName, Type type, Object initializationValue);

        /// <summary>
        ///     Add a global variable.
        ///     <para>
        ///         Use the runtime API to set variable values or EPL statements to change variable values.
        ///     </para>
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="initializationValue">
        ///     is the first assigned value.
        ///     For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can
        ///     be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be
        ///     Serializable.
        /// </param>
        /// <exception cref="ConfigurationException">
        ///     if the type and initialization value don't match or the variable name
        ///     is already in use
        /// </exception>
        void AddVariable<TValue>(string variableName, TValue initializationValue);

        /// <summary>
        ///     Add a global variable.
        ///     <para>
        ///         Use the runtime API to set variable values or EPL statements to change variable values.
        ///     </para>
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any
        ///     value or an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned value
        ///     For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can
        ///     be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be
        ///     Serializable.
        /// </param>
        /// <exception cref="ConfigurationException">
        ///     if the type and initialization value don't match or the variable name
        ///     is already in use
        /// </exception>
        void AddVariable(string variableName, string type, Object initializationValue);

        /// <summary>
        ///     Add a global variable, allowing constants.
        ///     <para>
        ///         Use the runtime API to set variable values or EPL statements to change variable values.
        ///     </para>
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any
        ///     value or an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned value
        ///     For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can
        ///     be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be
        ///     Serializable.
        /// </param>
        /// <param name="constant">true to identify the variable as a constant</param>
        /// <exception cref="ConfigurationException">
        ///     if the type and initialization value don't match or the variable name
        ///     is already in use
        /// </exception>
        void AddVariable(string variableName, string type, Object initializationValue, bool constant);

        /// <summary>
        ///     Adds an name for an event type that one of the plug-in event representations resolves to an event type.
        ///     <para>
        ///         The order of the URIs matters as event representations are asked in turn, to accept the event type.
        ///     </para>
        ///     <para>
        ///         URIs can be child URIs of plug-in event representations and can add additional parameters or fragments
        ///         for use by the event representation.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="resolutionURIs">is URIs that are matched to registered event representations</param>
        /// <param name="initializer">is an optional value for parameterizing or configuring the event type</param>
        void AddPlugInEventType(string eventTypeName, Uri[] resolutionURIs, object initializer);

        /// <summary>
        ///     Sets the URIs that point to plug-in event representations that are given a chance to dynamically resolve an event
        ///     type name to an event type, when a new (unseen) event type name occurs in a new EPL statement.
        ///     <para>
        ///         The order of the URIs matters as event representations are asked in turn, to accept the name.
        ///     </para>
        ///     <para>
        ///         URIs can be child URIs of plug-in event representations and can add additional parameters or fragments
        ///         for use by the event representation.
        ///     </para>
        /// </summary>
        /// <value>URIs for resolving the name</value>
        IList<Uri> PlugInEventTypeResolutionURIs { get; set; }

        /// <summary>
        ///     Adds an revision event type. The name of the event type may be used with named windows
        ///     to indicate that updates or new versions of events are processed.
        /// </summary>
        /// <param name="revisioneventTypeName">the name of the revision event type</param>
        /// <param name="revisionEventTypeConfig">the configuration</param>
        void AddRevisionEventType(string revisioneventTypeName, ConfigurationRevisionEventType revisionEventTypeConfig);

        /// <summary>
        ///     Adds a new variant stream. Variant streams allow events of disparate types to be treated the same.
        /// </summary>
        /// <param name="variantStreamName">is the name of the variant stream</param>
        /// <param name="variantStreamConfig">the configuration such as variant type names and any-type setting</param>
        void AddVariantStream(string variantStreamName, ConfigurationVariantStream variantStreamConfig);

        /// <summary>
        ///     Updates an existing Map event type with additional properties.
        ///     <para>
        ///         Does not update existing properties of the updated Map event type.
        ///     </para>
        ///     <para>
        ///         Adds additional nested properties to nesting levels, if any.
        ///     </para>
        ///     <para>
        ///         Each entry in the type mapping must contain the string property name of the additional property
        ///         and either a Type or further Map&lt;string, Object&gt; value for nested properties.
        ///     </para>
        ///     <para>
        ///         Map event types can only be updated at runtime, at configuration time updates are not allowed.
        ///     </para>
        ///     <para>
        ///         The type Map may list previously declared properties or can also contain only the new properties to be added.
        ///     </para>
        /// </summary>
        /// <param name="mapeventTypeName">the name of the map event type to update</param>
        /// <param name="typeMap">a Map of string property name and type</param>
        /// <exception cref="ConfigurationException">if the event type name could not be found or is not a Map</exception>
        void UpdateMapEventType(string mapeventTypeName, IDictionary<string, Object> typeMap);

        /// <summary>
        ///     Returns true if a variant stream by the name has been declared, or false if not.
        /// </summary>
        /// <param name="name">of variant stream</param>
        /// <returns>indicator whether the variant stream by that name exists</returns>
        bool IsVariantStreamExists(string name);

        /// <summary>
        ///     Sets a new interval for metrics reporting for a pre-configured statement group, or changes
        ///     the default statement reporting interval if supplying a null value for the statement group name.
        /// </summary>
        /// <param name="stmtGroupName">
        ///     name of statement group, provide a null value for the default statement interval (default
        ///     group)
        /// </param>
        /// <param name="newIntervalMSec">millisecond interval, use zero or negative value to disable</param>
        /// <exception cref="ConfigurationException">if the statement group cannot be found</exception>
        void SetMetricsReportingInterval(string stmtGroupName, long newIntervalMSec);

        /// <summary>
        ///     Enable metrics reporting for the given statement.
        ///     <para>
        ///         This operation can only be performed at runtime and is not available at engine initialization time.
        ///     </para>
        ///     <para>
        ///         Statement metric reporting follows the configured default or statement group interval.
        ///     </para>
        ///     <para>
        ///         Only if metrics reporting (on the engine level) has been enabled at initialization time
        ///         can statement-level metrics reporting be enabled through this method.
        ///     </para>
        /// </summary>
        /// <param name="statementName">for which to enable metrics reporting</param>
        /// <exception cref="ConfigurationException">if the statement cannot be found</exception>
        void SetMetricsReportingStmtEnabled(string statementName);

        /// <summary>
        ///     Disable metrics reporting for a given statement.
        /// </summary>
        /// <param name="statementName">for which to disable metrics reporting</param>
        /// <exception cref="ConfigurationException">if the statement cannot be found</exception>
        void SetMetricsReportingStmtDisabled(string statementName);

        /// <summary>
        ///     Enable engine-level metrics reporting.
        ///     <para>
        ///         Use this operation to control, at runtime, metrics reporting globally.
        ///     </para>
        ///     <para>
        ///         Only if metrics reporting (on the engine level) has been enabled at initialization time
        ///         can metrics reporting be re-enabled at runtime through this method.
        ///     </para>
        /// </summary>
        /// <exception cref="ConfigurationException">
        ///     if use at runtime and metrics reporting had not been enabled at initialization
        ///     time
        /// </exception>
        void SetMetricsReportingEnabled();

        /// <summary>
        ///     Disable engine-level metrics reporting.
        ///     <para>
        ///         Use this operation to control, at runtime, metrics reporting globally. Setting metrics reporting
        ///         to disabled removes all performance cost for metrics reporting.
        ///     </para>
        /// </summary>
        /// <exception cref="ConfigurationException">
        ///     if use at runtime and metrics reporting had not been enabled at initialization
        ///     time
        /// </exception>
        void SetMetricsReportingDisabled();

        /// <summary>
        ///     Remove an event type by its name, returning an indicator whether the event type was found and removed.
        ///     <para>
        ///         This method deletes the event type by it's name from the memory of the engine,
        ///         thereby allowing that the name to be reused for a new event type and disallowing new statements
        ///         that attempt to use the deleted name.
        ///     </para>
        ///     <para>
        ///         If there are one or more statements in started or stopped state that reference the event type,
        ///         this operation throws ConfigurationException unless the force flag is passed.
        ///     </para>
        ///     <para>
        ///         If using the force flag to remove the type while statements use the type, the exact
        ///         behavior of the engine depends on the event representation of the deleted event type and is thus
        ///         not well defined. It is recommended to destroy statements that use the type before removing the type.
        ///         Use #geteventTypeNameUsedBy to obtain a list of statements that use a type.
        ///     </para>
        ///     <para>
        ///         The method can be used for event types implicitly created for insert-into streams and for named windows.
        ///         The method does not remove variant streams and does not remove revision event types.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">the name of the event type to remove</param>
        /// <param name="force">
        ///     false to include a check that the type is no longer in use, true to force the remove
        ///     even though there can be one or more statements relying on that type
        /// </param>
        /// <exception cref="ConfigurationException">thrown to indicate that the remove operation failed</exception>
        /// <returns>indicator whether the event type was found and removed</returns>
        bool RemoveEventType(string eventTypeName, bool force);

        /// <summary>
        ///     Return the set of statement names of statements that are in started or stopped state and
        ///     that reference the given event type name.
        ///     <para>
        ///         A reference counts as any mention of the event type in a from-clause, a pattern, a insert-into or
        ///         as part of on-trigger.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">name of the event type</param>
        /// <returns>statement names referencing that type</returns>
        ICollection<string> GetEventTypeNameUsedBy(string eventTypeName);

        /// <summary>
        ///     Return the set of statement names of statements that are in started or stopped state and
        ///     that reference the given variable name.
        ///     <para>
        ///         A reference counts as any mention of the variable in any expression.
        ///     </para>
        /// </summary>
        /// <param name="variableName">name of the variable</param>
        /// <returns>statement names referencing that variable</returns>
        ICollection<string> GetVariableNameUsedBy(string variableName);

        /// <summary>
        ///     Remove a global non-context-partitioned variable by its name, returning an indicator whether the variable was found
        ///     and removed.
        ///     <para>
        ///         This method deletes the variable by it's name from the memory of the engine,
        ///         thereby allowing that the name to be reused for a new variable and disallowing new statements
        ///         that attempt to use the deleted name.
        ///     </para>
        ///     <para>
        ///         If there are one or more statements in started or stopped state that reference the variable,
        ///         this operation throws ConfigurationException unless the force flag is passed.
        ///     </para>
        ///     <para>
        ///         If using the force flag to remove the variable while statements use the variable, the exact
        ///         behavior is not well defined and affected statements may log errors.
        ///         It is recommended to destroy statements that use the variable before removing the variable.
        ///         Use #getVariableNameUsedBy to obtain a list of statements that use a variable.
        ///     </para>
        ///     <para>
        ///     </para>
        /// </summary>
        /// <param name="name">the name of the variable to remove</param>
        /// <param name="force">
        ///     false to include a check that the variable is no longer in use, true to force the remove
        ///     even though there can be one or more statements relying on that variable
        /// </param>
        /// <exception cref="ConfigurationException">thrown to indicate that the remove operation failed</exception>
        /// <returns>indicator whether the variable was found and removed</returns>
        bool RemoveVariable(string name, bool force);

        /// <summary>
        ///     Rebuild the XML event type based on changed type informaton, please read below for limitations.
        ///     <para>
        ///         Your application must ensure that the rebuild type information is compatible
        ///         with existing EPL statements and existing events.
        ///     </para>
        ///     <para>
        ///         The method can be used to change XPath expressions of existing attributes and to reload the schema and to add
        ///         attributes.
        ///     </para>
        ///     <para>
        ///         It is not recommended to remove attributes, change attribute type or change the root element name or namespace,
        ///         or to change type configuration other then as above.
        ///     </para>
        ///     <para>
        ///         If an existing EPL statement exists that refers to the event type then changes to the event type
        ///         do not become visible for those existing statements.
        ///     </para>
        /// </summary>
        /// <param name="xmlEventTypeName">the name of the XML event type</param>
        /// <param name="config">the new type configuration</param>
        /// <exception cref="ConfigurationException">thrown when the type information change failed</exception>
        void ReplaceXMLEventType(string xmlEventTypeName, ConfigurationEventTypeXMLDOM config);

        /// <summary>
        ///     Returns the event type for a given event type name. Returns null if a type by that name does not exist.
        ///     <para>
        ///         This operation is not available for static configuration and is only available for runtime use.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">to return event type for</param>
        /// <returns>event type or null if a type by that name does not exists</returns>
        EventType GetEventType(string eventTypeName);

        /// <summary>
        ///     Add an name for an event type that represents legacy type events.
        ///     <para>
        ///         This operation cannot be used to change an existing type.
        ///     </para>
        ///     <para>
        ///         Note that when adding multiple names for the same type the names represent an
        ///         alias to the same event type since event type identity for types is per type.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">class of the event type</typeparam>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        void AddEventType<T>(ConfigurationEventTypeLegacy legacyEventTypeDesc);

        /// <summary>
        ///     Add an name for an event type that represents legacy type events.
        ///     <para>
        ///         This operation cannot be used to change an existing type.
        ///     </para>
        ///     <para>
        ///         Note that when adding multiple names for the same type the names represent an
        ///         alias to the same event type since event type identity for types is per type.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">class of the event type</typeparam>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        void AddEventType<T>(string eventTypeName, ConfigurationEventTypeLegacy legacyEventTypeDesc);

        /// <summary>
        ///     Add an name for an event type that represents legacy type events.
        ///     <para>
        ///         This operation cannot be used to change an existing type.
        ///     </para>
        ///     <para>
        ///         Note that when adding multiple names for the same type the names represent an
        ///         alias to the same event type since event type identity for types is per type.
        ///     </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">assembly qualified class name of the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        void AddEventType(string eventTypeName, string eventClass, ConfigurationEventTypeLegacy legacyEventTypeDesc);
        void AddEventType(string eventTypeName, Type eventClass, ConfigurationEventTypeLegacy legacyEventTypeDesc);

        /// <summary>
        ///     Add a new plug-in view for use as a data window or derived value view.
        /// </summary>
        /// <param name="namespace">view namespace name</param>
        /// <param name="name">view name</param>
        /// <param name="viewFactoryClass">factory class of view</param>
        void AddPlugInView(string @namespace, string name, string viewFactoryClass);
        void AddPlugInView(string @namespace, string name, Type viewFactoryClass);

        /// <summary>
        ///     Set the current maximum pattern sub-expression count.
        ///     <para>
        ///         Use null to indicate that there is no current maximum.
        ///     </para>
        /// </summary>
        /// <value>to set</value>
        long PatternMaxSubexpressions { set; }

        /// <summary>
        ///     Set the current maximum match-recognize state count.
        ///     <para>
        ///         Use null to indicate that there is no current maximum.
        ///     </para>
        /// </summary>
        /// <value>to set</value>
        long? MatchRecognizeMaxStates { set; }

        /// <summary>
        ///     Updates an existing Object-array event type with additional properties.
        ///     <para>
        ///         Does not update existing properties of the updated Object-array event type.
        ///     </para>
        ///     <para>
        ///         Adds additional nested properties to nesting levels, if any.
        ///     </para>
        ///     <para>
        ///         Object-array event types can only be updated at runtime, at configuration time updates are not allowed.
        ///     </para>
        ///     <para>
        ///         The type properties may list previously declared properties or can also contain only the new properties to be
        ///         added.
        ///     </para>
        /// </summary>
        /// <param name="myEvent">the name of the object-array event type to update</param>
        /// <param name="namesNew">property names</param>
        /// <param name="typesNew">property types</param>
        /// <exception cref="ConfigurationException">if the event type name could not be found or is not a Map</exception>
        void UpdateObjectArrayEventType(string myEvent, string[] namesNew, Object[] typesNew);

        /// <summary>
        ///     Returns the transient configuration, which are configuration values that are passed by reference (and not by value)
        /// </summary>
        /// <value>transient configuration</value>
        IDictionary<string, object> TransientConfiguration { get; }

        /// <summary>
        ///     Adds an Avro event type
        /// </summary>
        /// <param name="eventTypeName">type name</param>
        /// <param name="avro">configs</param>
        void AddEventTypeAvro(string eventTypeName, ConfigurationEventTypeAvro avro);

        /// <summary>
        ///     Add a plug-in single-row function
        /// </summary>
        /// <param name="singleRowFunction">configuration</param>
        void AddPlugInSingleRowFunction(ConfigurationPlugInSingleRowFunction singleRowFunction);
    }

    public static class ConfigurationOperationsExtensions
    {
        public static void AddPlugInSingleRowFunction(
            this ConfigurationOperations configuration,
            string functionName,
            Type clazz,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions)
        {
            configuration.AddPlugInSingleRowFunction(
                functionName, clazz.AssemblyQualifiedName, methodName, valueCache,
                filterOptimizable, rethrowExceptions);
        }

        public static void AddPlugInSingleRowFunction(
            this ConfigurationOperations configuration,
            string functionName,
            Type clazz,
            string methodName,
            FilterOptimizableEnum filterOptimizable)
        {
            configuration.AddPlugInSingleRowFunction(
                functionName,
                clazz.AssemblyQualifiedName,
                methodName,
                filterOptimizable);
        }
    }
} // end of namespace