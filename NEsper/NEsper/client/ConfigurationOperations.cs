///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Provides configuration operations for configuration-time and runtime parameters.
    /// </summary>
    public interface ConfigurationOperations
    {
        /// <summary>
        /// Adds the namespace that event classes reside in.
        /// <para/>
        /// This setting allows an application to place all it's events into one or more namespaces and then
        /// declare these packages via this method. The engine attempts to resolve an event type name to a class
        /// residing in each declared package.
        /// <para/>
        /// For example, in the statement "select * from MyEvent" the engine attempts to load class "namespace.MyEvent"
        /// and if successful, uses that class as the event type.
        /// </summary>
        /// <param name="packageName">is the fully-qualified the namespace that event classes reside in</param>
        void AddEventTypeAutoName(String packageName);

        /// <summary>
        /// Adds a plug-in aggregation multi-function.
        /// </summary>
        /// <param name="config">The config.</param>
        void AddPlugInAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction config);

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para/>
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryClassName">is the fully-qualified class name of the class implementing the aggregation function factory interface <seealso cref="AggregationFunctionFactory"/></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        void AddPlugInAggregationFunctionFactory(String functionName, String aggregationFactoryClassName) ;

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryType">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        void AddPlugInAggregationFunctionFactory(String functionName, Type aggregationFactoryType);

        /// <summary>
        /// Adds the plug in aggregation function factory.
        /// </summary>
        /// <typeparam name="T">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></typeparam>
        /// <param name="functionName">Name of the function.</param>
        void AddPlugInAggregationFunctionFactory<T>(String functionName) where T : AggregationFunctionFactory;

        /// <summary>
        /// Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache behavior. <para /> The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <param name="valueCache">set the behavior for caching the return value when constant parameters are provided</param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the single-row function</throws>
        void AddPlugInSingleRowFunction(String functionName, String className, String methodName, ValueCache valueCache);

        /// <summary>
        /// Adds a plug-in single-row function given a EPL function name, a class name, method name and setting for value-cache behavior. <para /> The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <param name="filterOptimizable">whether the single-row function, when used in filters, may be subject to reverse index lookup based on the function result</param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the single-row function</throws>
        void AddPlugInSingleRowFunction(String functionName, String className, String methodName, FilterOptimizable filterOptimizable);

        /// <summary>
        /// Adds a plug-in single-row function given a EPL function name, a class name and a method name.
        /// <para/>
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the single-row function</throws>
        void AddPlugInSingleRowFunction(String functionName, String className, String methodName);

        /// <summary>
        /// Adds a plug-in single-row function given a EPL function name, a class name, method name and a setting for value-cache behavior.
        /// <para/>
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new single-row function name for use in EPL</param>
        /// <param name="className">is the fully-qualified class name of the class implementing the single-row function</param>
        /// <param name="methodName">is the public static method provided by the class that implements the single-row function</param>
        /// <param name="valueCache">The value cache.</param>
        /// <param name="filterOptimizable">The filter optimizable.</param>
        /// <param name="rethrowExceptions">if set to <c>true</c> [rethrow exceptions].</param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the single-row function</throws>
        void AddPlugInSingleRowFunction(String functionName,
                                        String className,
                                        String methodName,
                                        ValueCache valueCache,
                                        FilterOptimizable filterOptimizable,
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
        /// Adds an import for the namespace associated with the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddNamespaceImport<T>();

        /// <summary>Checks if an eventTypeName has already been registered for that name. </summary>
        /// <unknown>@since 2.1</unknown>
        /// <param name="eventTypeName">the name</param>
        /// <returns>true if already registered</returns>
        bool IsEventTypeExists(String eventTypeName);

        /// <summary>
        /// Add an name for an event type represented by object events.
        /// <para/>
        /// Allows a second name to be added for the same type.
        /// Does not allow the same name to be used for different types.
        /// <para>
        /// Note that when adding multiple names for the same type the names represent an
        /// alias to the same event type since event type identity for types is per type.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="nativeEventTypeName">fully-qualified class name of the event type</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, String nativeEventTypeName);

        /// <summary>
        /// Add an name for an event type represented by plain-old object events.
        /// <para/>
        /// Allows a second name to be added for the same type.
        /// Does not allow the same name to be used for different types.
        /// <para>
        /// Note that when adding multiple names for the same type the names represent an
        /// alias to the same event type since event type identity for types is per type.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventType">is the event class for which to create the name</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, Type eventType);

        /// <summary>
        /// Add a name for an event type represented by plain-old object events, using the simple name of the
        /// class as the name.
        /// <para/>
        /// For example, if your class is "com.mycompany.MyEvent", then this method adds the name "MyEvent" for the class.
        /// <para/>
        /// Allows a second name to be added for the same type. Does not allow the same name to be used for different types.
        /// </summary>
        /// <param name="eventType">is the event class for which to create the name from the class simple name</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(Type eventType);

        /// <summary>
        /// Adds a name for an event type represented by the type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventTypeName">Name of the event type.</param>
        void AddEventType<T>(String eventTypeName);

        /// <summary>
        /// Adds a name for an event type represented by the type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void AddEventType<T>();

        /// <summary>
        /// Add an event for an event type that represents map events.
        /// <para/>
        /// Allows a second name to be added for the same type. Does not allow the same name to be used for different types.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">maps the name of each property in the Map event to the Type(fully qualified classname) of
        /// its value in Map event instances.</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, Properties typeMap);

        /// <summary>
        /// Add an event type that represents Object-array (Object[]) events.
        /// </summary>
        /// <param name="eventTypeName">Name of the event type.</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>

        void AddEventType(String eventTypeName, String[] propertyNames, Object[] propertyTypes);


        /// <summary>
        /// Add an event type that represents Object-array (Object[]) events.
        /// <para>
        /// Note that when adding multiple names for the same type the names represent an
        /// alias to the same event type since event type identity for types is per type.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">the name for the event type</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>
        /// <param name="optionalConfiguration">object-array type configuration</param>
        void AddEventType(String eventTypeName,
                          String[] propertyNames,
                          Object[] propertyTypes,
                          ConfigurationEventTypeObjectArray optionalConfiguration);

        /// <summary>
        /// Add an name for an event type that represents map events, and for which each property may itself be a Map
        /// of further properties, with unlimited nesting levels. <para/> Each entry in the type mapping must contain
        /// the String property name as the key value, and either a Class, or a further Map&lt;String, Object&gt;, or
        /// the name of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">maps the name of each property in the Map event to the Type(fully qualified classname) of its value in Map event instances.</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap);

        /// <summary>
        /// Add a name for an event type that represents Map events,
        /// and for which each property may itself be a Map of further properties,
        /// with unlimited nesting levels.
        /// <para/>
        /// Each entry in the type mapping must contain the String property name as the key value,
        /// and either a Class, or a further Dictionary, or the name
        /// of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="mapConfig">is the Map-event type configuration that may defined super-types, timestamp-property-name etc.</param>
        /// <param name="typeMap">maps the name of each property in the Map event to the type</param>
        void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, ConfigurationEventTypeMap mapConfig);

        /// <summary>
        /// Add an name for an event type that represents map events, and for which each property may itself be a Map
        /// of further properties, with unlimited nesting levels.
        /// <para/>
        /// Each entry in the type mapping must contain the String property name as the key value, and either a Class,
        /// or a further Map&lt;String, Object&gt;, or the name of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">maps the name of each property in the Map event to the Type(fully qualified classname)
        /// of its value in Map event instances.</param>
        /// <param name="superTypes">is an array of event type name of further Map types that this</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, String[] superTypes);

        /// <summary>
        /// Add an name for an event type that represents XmlNode events.
        /// <para/> Allows a second name to be added for the same type. Does not allow the same name to be used for different types.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="xmlDOMEventTypeDesc">descriptor containing property and mapping information for XML-DOM events</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        void AddEventType(String eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc);

        /// <summary>
        /// Add a global variable.
        /// <para/>
        /// Use the runtime API to set variable values or EPL statements to change variable values.
        /// <para />
        /// For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can be string-typed and will be parsed.
        /// For static initialization the initialization value, if provided, must be Serializable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="initializationValue">is the first assigned value</param>
        /// <throws>ConfigurationException if the type and initialization value don't match or the variable nameis already in use </throws>
        void AddVariable<T>(String variableName, T initializationValue);

        /// <summary>
        /// Add a global variable.
        /// <para />
        /// Use the runtime API to set variable values or EPL statements to change variable values.
        /// <para />
        /// For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can be string-typed and will be parsed.
        /// For static initialization the initialization value, if provided, must be Serializable.
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any value an event type name or a class name or fully-qualified class name.  Append "[]" for array.</param>
        /// <param name="initializationValue">is the first assigned value</param>
        /// <throws>ConfigurationException if the type and initialization value don't match or the variable nameis already in use </throws>
        void AddVariable(String variableName, Type type, Object initializationValue);

        /// <summary>
        /// Add a global variable, allowing constants.
        /// <para/>
        /// Use the runtime API to set variable values or EPL statements to change variable values.
        /// <para />
        /// For static initialization via the <seealso cref="com.espertech.esper.client.Configuration" /> object the value can be string-typed and will be parsed.
        /// For static initialization the initialization value, if provided, must be Serializable.
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any value an event type name or a class name or fully-qualified class name.  Append "[]" for array.</param>
        /// <param name="initializationValue">is the first assigned value</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        /// <throws>ConfigurationException if the type and initialization value don't match or the variable nameis already in use </throws>
        void AddVariable(String variableName, String type, Object initializationValue, bool constant = false);

        /// <summary>
        /// Adds an name for an event type that one of the plug-in event representations resolves to an event type.
        /// <para/> The order of the URIs matters as event representations are asked in turn, to accept the event type.
        /// <para/> URIs can be child URIs of plug-in event representations and can add additional parameters or fragments
        /// for use by the event representation.
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="resolutionURIs">is URIs that are matched to registered event representations</param>
        /// <param name="initializer">is an optional value for parameterizing or configuring the event type</param>
        void AddPlugInEventType(String eventTypeName, IList<Uri> resolutionURIs, Object initializer);

        /// <summary>
        /// Sets the URIs that point to plug-in event representations that are given a chance to dynamically resolve
        /// an event type name to an event type, when a new (unseen) event type name occurs in a new EPL statement.
        /// <para/> The order of the URIs matters as event representations are asked in turn, to accept the name.
        /// <para/> URIs can be child URIs of plug-in event representations and can add additional parameters or fragments
        /// for use by the event representation.
        /// </summary>
        IList<Uri> PlugInEventTypeResolutionURIs { get; set; }

        /// <summary>
        /// Adds an revision event type. The name of the event type may be used with named windows to indicate that
        /// updates or new versions of events are processed.
        /// </summary>
        /// <param name="revisionEventTypeName">the name of the revision event type</param>
        /// <param name="revisionEventTypeConfig">the configuration</param>
        void AddRevisionEventType(String revisionEventTypeName, ConfigurationRevisionEventType revisionEventTypeConfig);

        /// <summary>
        /// Adds a new variant stream. Variant streams allow events of disparate types to be treated the same.
        /// </summary>
        /// <param name="variantEventTypeName">is the name of the variant stream</param>
        /// <param name="variantStreamConfig">the configuration such as variant type names and any-type setting</param>
        void AddVariantStream(String variantEventTypeName, ConfigurationVariantStream variantStreamConfig);

        /// <summary>
        /// Updates an existing Map event type with additional properties.
        /// <para/> Does not Update existing properties of the updated Map event type.
        /// <para/> Adds additional nested properties to nesting levels, if any.
        /// <para/> Each entry in the type mapping must contain the String property name of the additional property
        /// and either a Class or further Map&lt;String, Object&gt; value for nested properties.
        /// <para/> Map event types can only be updated at runtime, at configuration time updates are not allowed.
        /// <para/> The type Map may list previously declared properties or can also contain only the new properties to be added.
        /// </summary>
        /// <param name="mapeventTypeName">the name of the map event type to Update</param>
        /// <param name="typeMap">a Map of string property name and type</param>
        /// <throws>ConfigurationException if the event type name could not be found or is not a Map</throws>
        void UpdateMapEventType(String mapeventTypeName, IDictionary<String, Object> typeMap);

        /// <summary>
        /// Returns true if a variant stream by the name has been declared, or false if not.
        /// </summary>
        /// <param name="name">of variant stream</param>
        /// <returns>
        /// indicator whether the variant stream by that name exists
        /// </returns>
        bool IsVariantStreamExists(String name);

        /// <summary>
        /// Sets a new interval for metrics reporting for a pre-configured statement group, or changes the default statement
        /// reporting interval if supplying a null value for the statement group name.
        /// </summary>
        /// <param name="stmtGroupName">name of statement group, provide a null value for the default statement interval (default group)</param>
        /// <param name="newIntervalMSec">millisecond interval, use zero or negative value to disable</param>
        /// <throws>ConfigurationException if the statement group cannot be found</throws>
        void SetMetricsReportingInterval(String stmtGroupName, long newIntervalMSec);

        /// <summary>
        /// Enable metrics reporting for the given statement.
        /// <para/> This operation can only be performed at runtime and is not available at engine initialization time.
        /// <para/> Statement metric reporting follows the configured default or statement group interval.
        /// <para/> Only if metrics reporting (on the engine level) has been enabled at initialization time can statement-level
        /// metrics reporting be enabled through this method.
        /// </summary>
        /// <param name="statementName">for which to enable metrics reporting</param>
        /// <throws>ConfigurationException if the statement cannot be found</throws>
        void SetMetricsReportingStmtEnabled(String statementName);

        /// <summary>
        /// Disable metrics reporting for a given statement.
        /// </summary>
        /// <param name="statementName">for which to disable metrics reporting</param>
        /// <throws>ConfigurationException if the statement cannot be found</throws>
        void SetMetricsReportingStmtDisabled(String statementName);

        /// <summary>
        /// Enable engine-level metrics reporting.
        /// <para/> Use this operation to control, at runtime, metrics reporting globally.
        /// <para/> Only if metrics reporting (on the engine level) has been enabled at initialization time can metrics
        /// reporting be re-enabled at runtime through this method.
        /// </summary>
        /// <throws>ConfigurationException if use at runtime and metrics reporting had not been enabled at initialization time</throws>
        void SetMetricsReportingEnabled();

        /// <summary>
        /// Disable engine-level metrics reporting.
        /// <para/> Use this operation to control, at runtime, metrics reporting globally. Setting metrics reporting to
        /// disabled removes all performance cost for metrics reporting.
        /// </summary>
        /// <throws>ConfigurationException if use at runtime and metrics reporting had not been enabled at initialization time</throws>
        void SetMetricsReportingDisabled();

        /// <summary>
        /// Remove an event type by its name, returning an indicator whether the event type was found and removed.
        /// <para/> This method deletes the event type by it's name from the memory of the engine, thereby allowing that
        /// the name to be reused for a new event type and disallowing new statements that attempt to use the deleted name.
        /// <para/> If there are one or more statements in started or stopped state that reference the event type, this
        /// operation throws ConfigurationException unless the force flag is passed.
        /// <para/> If using the force flag to remove the type while statements use the type, the exact behavior of the
        /// engine depends on the event representation of the deleted event type and is thus not well defined. It is
        /// recommended to destroy statements that use the type before removing the type. Use #geteventTypeNameUsedBy
        /// to obtain a list of statements that use a type. <para/> The method can be used for event types implicitly
        /// created for insert-into streams and for named windows. The method does not remove variant streams and does
        /// not remove revision event types.
        /// </summary>
        /// <param name="name">the name of the event type to remove</param>
        /// <param name="force">false to include a check that the type is no longer in use, true to force the remove
        /// even though there can be one or more statements relying on that type</param>
        /// <returns>
        /// indicator whether the event type was found and removed
        /// </returns>
        /// <throws>ConfigurationException thrown to indicate that the remove operation failed</throws>
        bool RemoveEventType(String name, bool force);

        /// <summary>
        /// Return the set of statement names of statements that are in started or stopped state and that reference
        /// the given event type name. <para/> A reference counts as any mention of the event type in a from-clause,
        /// a pattern, a insert-into or as part of on-trigger.
        /// </summary>
        /// <param name="eventTypeName">name of the event type</param>
        /// <returns>statement names referencing that type</returns>
        ICollection<String> GetEventTypeNameUsedBy(String eventTypeName);

        /// <summary>
        /// Return the set of statement names of statements that are in started or stopped state and that reference 
        /// the given variable name.
        /// <para/>
        /// A reference counts as any mention of the variable in any expression.
        /// </summary>
        /// <param name="variableName">name of the variable</param>
        /// <returns>
        /// statement names referencing that variable
        /// </returns>
        ICollection<String> GetVariableNameUsedBy(String variableName);

        /// <summary>
        /// Remove a global non-context-partitioned variable by its name, returning an indicator whether the variable was found and removed.
        /// <para/>
        /// This method deletes the variable by it's name from the memory of the engine, thereby allowing that
        /// the name to be reused for a new variable and disallowing new statements that attempt to use the
        /// deleted name.
        /// <para/>
        /// If there are one or more statements in started or stopped state that reference the variable, this
        /// operation throws ConfigurationException unless the force flag is passed.
        /// <para/>
        /// If using the force flag to remove the variable while statements use the variable, the exact behavior
        /// is not well defined and affected statements may log errors. It is recommended to destroy statements
        /// that use the variable before removing the variable. Use #getVariableNameUsedBy to obtain a list of
        /// statements that use a variable.
        /// </summary>
        /// <param name="name">the name of the variable to remove</param>
        /// <param name="force">false to include a check that the variable is no longer in use, true to force the removeeven though there can be one or more statements relying on that variable</param>
        /// <returns>
        /// indicator whether the variable was found and removed
        /// </returns>
        /// <throws>ConfigurationException thrown to indicate that the remove operation failed</throws>
        bool RemoveVariable(String name, bool force);

        /// <summary>
        /// Rebuild the XML event type based on changed type informaton, please read below 
        /// for limitations. 
        /// <para/>
        /// Your application must ensure that the rebuild type information is compatible with existing 
        /// EPL statements and existing events. 
        /// <para/>
        /// The method can be used to change XPath expressions of existing attributes and to reload the 
        /// schema and to add attributes.
        /// <para/>
        /// It is not recommended to remove attributes, change attribute type or change the root element name 
        /// or namespace, or to change type configuration other then as above. 
        /// <para/>
        /// If an existing EPL statement exists that refers to the event type then changes to the event type 
        /// do not become visible for those existing statements.
        /// </summary>
        /// <param name="xmlEventTypeName">the name of the XML event type</param>
        /// <param name="config">the new type configuration</param>
        /// <throws>ConfigurationException thrown when the type information change failed</throws>
        void ReplaceXMLEventType(String xmlEventTypeName, ConfigurationEventTypeXMLDOM config);

        /// <summary>
        /// Returns the event type for a given event type name. Returns null if a type by that name 
        /// does not exist.
        /// <para/>
        /// This operation is not available for static configuration and is only available for runtime use.
        /// </summary>
        /// <param name="eventTypeName">to return event type for</param>
        /// <returns>
        /// event type or null if a type by that name does not exists
        /// </returns>
        EventType GetEventType(String eventTypeName);

        /// <summary>
        /// Returns an array of event types tracked or available within the engine in any order. Included
        /// are all application-configured or EPL-created schema types as well as dynamically-allocated 
        /// stream's event types or types otherwise known to the engine as a dependeny type or supertype 
        /// to another type.
        /// <para/>
        /// Event types that are associated to statement output may not necessarily be returned as such 
        /// types, depending on the statement, are considered anonymous.
        /// <para/>
        /// This operation is not available for static configuration and is only available for runtime use.
        /// </summary>
        /// <value>event type array</value>
        ICollection<EventType> EventTypes { get; }

        /// <summary>
        /// Add an name for an event type that represents legacy type (non-object style) events.
        /// <para/>
        /// This operation cannot be used to change an existing type.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventType">fully-qualified class name of the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        void AddEventType(String eventTypeName, String eventType, ConfigurationEventTypeLegacy legacyEventTypeDesc);

        /// <summary>Add a new plug-in view for use as a data window or derived value view. </summary>
        /// <param name="namespace">view namespace name</param>
        /// <param name="name">view name</param>
        /// <param name="viewFactoryClass">factory class of view</param>
        void AddPlugInView(String @namespace, String name, String viewFactoryClass);

        /// <summary>
        /// Gets or sets the current maximum pattern sub-expression count.
        /// <para/>
        /// Use null to indicate that there is not current maximum.
        /// </summary>
        /// <value>to set</value>
        long PatternMaxSubexpressions { get; set; }

        /// <summary>
        /// Gets or sets the current maximum match-recognize state count.
        /// <para>
        /// Use null to indicate that there is no current maximum.
        /// </para>
        /// </summary>
        long? MatchRecognizeMaxStates { get; set; }

        /// <summary>
        /// Updates an existing Object-array event type with additional properties. 
        /// <para>
        /// Does not Update existing properties of the updated Object-array event type.
        /// </para>
        /// <para>
        /// Adds additional nested properties to nesting levels, if any. 
        /// </para>
        /// <para>
        /// Object-array event types can only be updated at runtime, at configuration 
        /// time updates are not allowed.
        /// </para>
        /// <para>
        /// The type properties may list previously declared properties or can also 
        /// contain only the new properties to be added.
        /// </para>
        /// </summary>
        /// <param name="myEvent">the name of the object-array event type to Update</param>
        /// <param name="namesNew">property names</param>
        /// <param name="typesNew">property types</param>
        /// <throws>ConfigurationException if the event type name could not be found or is not a Map</throws>
        void UpdateObjectArrayEventType(String myEvent, String[] namesNew, Object[] typesNew);
    }
}
