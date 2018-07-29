///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    /// <summary>
    /// An instance of <tt>Configuration</tt> allows the application
    /// to specify properties to be used when
    /// creating a <tt>EPServiceProvider</tt>. Usually an application will create
    /// a single <tt>Configuration</tt>, then get one or more instances of
    /// <seealso cref="EPServiceProvider" /> via <seealso cref="EPServiceProviderManager" />.
    /// The <tt>Configuration</tt> is meant
    /// only as an initialization-time object. <tt>EPServiceProvider</tt>s are
    /// immutable and do not retain any association back to the
    /// <tt>Configuration</tt>.
    /// <para>
    /// The format of an Esper XML configuration file is defined in
    /// <tt>esper-configuration-(version).xsd</tt>.
    /// </para>
    /// </summary>
    [Serializable]
    public class Configuration
        : ConfigurationOperations
        , ConfigurationInformation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Default name of the configuration file.</summary>
        internal static readonly string ESPER_DEFAULT_CONFIG = "esper.cfg.xml";

        /// <summary>
        /// Import name of the package that hosts the annotation classes.
        /// </summary>
        public static readonly String ANNOTATION_IMPORT = typeof(NameAttribute).Namespace;

        /// <summary>Map of event name and fully-qualified class name.</summary>
        private IDictionary<string, string> _eventClasses;
        /// <summary>Map of event type name and XML DOM configuration.</summary>
        private IDictionary<string, ConfigurationEventTypeXMLDOM> _eventTypesXmldom;
        /// <summary>Map of event type name and XML DOM configuration.</summary>
        private IDictionary<string, ConfigurationEventTypeAvro> _eventTypesAvro;
        /// <summary>Map of event type name and Legacy-type event configuration.</summary>
        private IDictionary<string, ConfigurationEventTypeLegacy> _eventTypesLegacy;
        /// <summary>
        /// The type names for events that are backed by IDictionary,
        /// not containing strongly-typed nested maps.
        /// </summary>
        private IDictionary<string, Properties> _mapNames;
        /// <summary>
        /// The type names for events that are backed by IDictionary,
        /// possibly containing strongly-typed nested maps.
        /// <para>
        /// Each entrie's value must be either a Type or a Map&lt;string,Object&gt; to
        /// define nested maps.
        /// </para>
        /// </summary>
        private IDictionary<string, IDictionary<string, Object>> _nestableMapNames;
        /// <summary>
        /// The type names for events that are backed by IDictionary,
        /// possibly containing strongly-typed nested maps.
        /// <para>
        /// Each entrie's value must be either a Type or a Map&lt;string,Object&gt; to
        /// define nested maps.
        /// </para>
        /// </summary>
        private IDictionary<string, IDictionary<string, Object>> _nestableObjectArrayNames;
        /// <summary>Map event types additional configuration information.</summary>
        private IDictionary<string, ConfigurationEventTypeMap> _mapTypeConfigurations;
        /// <summary>Map event types additional configuration information.</summary>
        private IDictionary<string, ConfigurationEventTypeObjectArray> _objectArrayTypeConfigurations;
        /// <summary>
        /// The class and package name imports that
        /// will be used to resolve partial class names.
        /// </summary>
        private IList<AutoImportDesc> _imports;
        /// <summary>
        /// For annotations only, the class and package name imports that
        /// will be used to resolve partial class names (not available in EPL statements unless used in an annotation).
        /// </summary>
        private IList<AutoImportDesc> _annotationImports;
        /// <summary>
        /// The class and package name imports that
        /// will be used to resolve partial class names.
        /// </summary>
        private IDictionary<string, ConfigurationDBRef> _databaseReferences;
        /// <summary>
        /// Optional classname to use for constructing services context.
        /// </summary>
        private string _epServicesContextFactoryClassName;
        /// <summary>List of configured plug-in views.</summary>
        private IList<ConfigurationPlugInView> _plugInViews;
        /// <summary>List of configured plug-in views.</summary>
        private IList<ConfigurationPlugInVirtualDataWindow> _plugInVirtualDataWindows;
        /// <summary>List of configured plug-in pattern objects.</summary>
        private IList<ConfigurationPlugInPatternObject> _plugInPatternObjects;
        /// <summary>List of configured plug-in aggregation functions.</summary>
        private IList<ConfigurationPlugInAggregationFunction> _plugInAggregationFunctions;
        /// <summary>List of configured plug-in aggregation multi-functions.</summary>
        private IList<ConfigurationPlugInAggregationMultiFunction> _plugInAggregationMultiFunctions;
        /// <summary>List of configured plug-in single-row functions.</summary>
        private IList<ConfigurationPlugInSingleRowFunction> _plugInSingleRowFunctions;
        /// <summary>List of adapter loaders.</summary>
        private IList<ConfigurationPluginLoader> _pluginLoaders;
        /// <summary>Saves engine default configs such as threading settings</summary>
        private ConfigurationEngineDefaults _engineDefaults;
        /// <summary>Saves the packages to search to resolve event type names.</summary>
        private ISet<string> _eventTypeAutoNamePackages;
        /// <summary>Map of variables.</summary>
        private IDictionary<string, ConfigurationVariable> _variables;
        /// <summary>
        /// Map of class name and configuration for method invocations on that class.
        /// </summary>
        private IDictionary<string, ConfigurationMethodRef> _methodInvocationReferences;
        /// <summary>Map of plug-in event representation name and configuration</summary>
        private IDictionary<Uri, ConfigurationPlugInEventRepresentation> _plugInEventRepresentation;
        /// <summary>Map of plug-in event types.</summary>
        private IDictionary<string, ConfigurationPlugInEventType> _plugInEventTypes;
        /// <summary>
        /// Uris that point to plug-in event representations that are given a chance to dynamically resolve an event type name to an
        /// event type, as it occurs in a new EPL statement.
        /// </summary>
        private IList<Uri> _plugInEventTypeResolutionUris;
        /// <summary>
        /// All revision event types which allow updates to past events.
        /// </summary>
        private IDictionary<string, ConfigurationRevisionEventType> _revisionEventTypes;
        /// <summary>
        /// Variant streams allow events of disparate types to be treated the same.
        /// </summary>
        private IDictionary<string, ConfigurationVariantStream> _variantStreams;
        [NonSerialized] private IDictionary<string, Object> _transientConfiguration;
        [NonSerialized] private IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            _container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();
            Reset();
        }

        /// <summary>
        /// Constructs an empty configuration. The auto import values
        /// are set by default to System, System.Collections and
        /// System.Text.
        /// </summary>
        /// <param name="container">The container.</param>
        public Configuration(IContainer container)
        {
            _container = container;
            Reset();
        }

        /// <summary>
        /// Get the configuration file as an <tt>InputStream</tt>. Might be overridden
        /// by subclasses to allow the configuration to be located by some arbitrary
        /// mechanism.
        /// <para>
        /// See <tt>getResourceAsStream</tt> for information on how the resource name is resolved.
        /// </para>
        /// </summary>
        /// <param name="resource">is the resource name</param>
        /// <exception cref="EPException">thrown to indicate error reading configuration</exception>
        /// <returns>input stream for resource</returns>
        internal Stream GetConfigurationInputStream(string resource)
        {
            return GetResourceAsStream(resource);
        }

        /// <summary>
        /// Returns an input stream from an application resource in the classpath.
        /// <para>
        /// The method first removes the '/' character from the resource name if
        /// the first character is '/'.
        /// </para>
        /// <para>
        /// The lookup order is as follows:
        /// </para>
        /// <para>
        /// If a thread context class loader exists, use <tt>Thread.CurrentThread().getResourceAsStream</tt>
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, use the <tt>typeof(Configuration).getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, use the <tt>typeof(Configuration).ClassLoader.getResourceAsStream</tt>.
        /// to obtain an InputStream.
        /// </para>
        /// <para>
        /// If no input stream was returned, throw an Exception.
        /// </para>
        /// </summary>
        /// <param name="resource">to get input stream for</param>
        /// <returns>input stream for resource</returns>
        internal Stream GetResourceAsStream(String resource)
        {
            String stripped = resource.StartsWith("/", StringComparison.CurrentCultureIgnoreCase)
                ? resource.Substring(1) : resource;
            Stream stream = ResourceManager.GetResourceAsStream(resource) ??
                            ResourceManager.GetResourceAsStream(stripped);
            if (stream == null)
            {
                throw new EPException(resource + " not found");
            }
            return stream;
        }

        public string EPServicesContextFactoryClassName => _epServicesContextFactoryClassName;
        /// <summary>
        /// Sets the class name of the services context factory class to use.
        /// </summary>
        /// <param name="epServicesContextFactoryClassName">service context factory class name</param>
        public void SetEPServicesContextFactoryClassName(string epServicesContextFactoryClassName)
        {
            _epServicesContextFactoryClassName = epServicesContextFactoryClassName;
        }

        public void AddPlugInAggregationFunctionFactory(string functionName, string aggregationFactoryClassName)
        {
            var entry = new ConfigurationPlugInAggregationFunction();
            entry.Name = functionName;
            entry.FactoryClassName = aggregationFactoryClassName;
            _plugInAggregationFunctions.Add(entry);
        }

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryClass">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionFactory(String functionName, Type aggregationFactoryClass)
        {
            AddPlugInAggregationFunctionFactory(functionName, aggregationFactoryClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Adds the plug in aggregation function factory.
        /// </summary>
        /// <typeparam name="T">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></typeparam>
        /// <param name="functionName">Name of the function.</param>
        public void AddPlugInAggregationFunctionFactory<T>(String functionName) where T : AggregationFunctionFactory
        {
            AddPlugInAggregationFunctionFactory(functionName, typeof(T).AssemblyQualifiedName);
        }

        public void AddPlugInAggregationMultiFunction(ConfigurationPlugInAggregationMultiFunction config)
        {
            _plugInAggregationMultiFunctions.Add(config);
        }

        public void AddPlugInSingleRowFunction(ConfigurationPlugInSingleRowFunction singleRowFunction)
        {
            _plugInSingleRowFunctions.Add(singleRowFunction);
        }

        public void AddPlugInSingleRowFunction(string functionName, Type clazz, string methodName)
        {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName);
        }

        public void AddPlugInSingleRowFunction(string functionName, string className, string methodName)
        {
            AddPlugInSingleRowFunction(
                functionName, className, methodName, ValueCacheEnum.DISABLED);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ValueCacheEnum valueCache)
        {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, valueCache);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ValueCacheEnum valueCache)
        {
            AddPlugInSingleRowFunction(
                functionName, className, methodName, valueCache, FilterOptimizableEnum.ENABLED);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, filterOptimizable);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(
                functionName, className, methodName, ValueCacheEnum.DISABLED, filterOptimizable);
        }

        /// <summary>
        /// Returns transient configuration, i.e. information that is passed along as a reference and not as a value
        /// </summary>
        /// <value>map of transients</value>
        public IDictionary<string, object> TransientConfiguration
        {
            get => _transientConfiguration;
            set => _transientConfiguration = value;
        }

        public IContainer Container
        {
            get => _container;
            set => _container = value;
        }

        public IResourceManager ResourceManager => _container.ResourceManager();
        /// <summary>
        /// Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <exception cref="ConfigurationException">thrown to indicate that the configuration is invalid</exception>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable)
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, valueCache, filterOptimizable, false);
        }

        public void AddPlugInSingleRowFunction(
            string functionName,
            Type clazz,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions)
        {
            AddPlugInSingleRowFunction(functionName, clazz.AssemblyQualifiedName, methodName, valueCache, filterOptimizable, rethrowExceptions);
        }

        /// <summary>
        /// Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <param name="rethrowExceptions">whether exceptions generated by the UDF are rethrown</param>
        /// <exception cref="ConfigurationException">thrown to indicate that the configuration is invalid</exception>
        public void AddPlugInSingleRowFunction(
            string functionName,
            string className,
            string methodName,
            ValueCacheEnum valueCache,
            FilterOptimizableEnum filterOptimizable,
            bool rethrowExceptions)
        {
            var entry = new ConfigurationPlugInSingleRowFunction();
            entry.FunctionClassName = className;
            entry.FunctionMethodName = methodName;
            entry.Name = functionName;
            entry.ValueCache = valueCache;
            entry.FilterOptimizable = filterOptimizable;
            entry.IsRethrowExceptions = rethrowExceptions;
            AddPlugInSingleRowFunction(entry);
        }

        /// <summary>
        /// Checks if an event type has already been registered for that name.
        /// </summary>
        /// <param name="eventTypeName">the name</param>
        /// <returns>true if already registered</returns>
        public bool IsEventTypeExists(string eventTypeName)
        {
            return _eventClasses.ContainsKey(eventTypeName)
                   || _mapNames.ContainsKey(eventTypeName)
                   || _nestableMapNames.ContainsKey(eventTypeName)
                   || _nestableObjectArrayNames.ContainsKey(eventTypeName)
                   || _eventTypesXmldom.ContainsKey(eventTypeName)
                   || _eventTypesAvro.ContainsKey(eventTypeName);
            //note: no need to check legacy as they get added as class event type
        }

        /// <summary>
        /// Add an name for an event type represented by object events.
        /// Note that when adding multiple names for the same class the names represent an
        /// alias to the same event type since event type identity for classes is per class.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClassName">fully-qualified class name of the event type</param>
        public void AddEventType(string eventTypeName, string eventClassName)
        {
            _eventClasses[eventTypeName] = eventClassName;
        }

        /// <summary>
        /// Add an name for an event type represented by plain-old object events.
        /// Note that when adding multiple names for the same class the names represent an
        /// alias to the same event type since event type identity for classes is per class.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">is the event class for which to add the name</param>
        public void AddEventType(string eventTypeName, Type eventClass)
        {
            AddEventType(eventTypeName, eventClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Add event type represented by plain-old object events,
        /// and the name is the simple class name of the class.
        /// </summary>
        /// <param name="eventClass">is the event class for which to add the name</param>
        public void AddEventType(Type eventClass)
        {
            AddEventType(eventClass.Name, eventClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Adds a name for an event type represented by the type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventTypeName">Name of the event type.</param>
        public void AddEventType<T>(String eventTypeName)
        {
            AddEventType(eventTypeName, typeof(T).AssemblyQualifiedName);
        }

        /// <summary>
        /// Adds a name for an event type represented by the type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddEventType<T>()
        {
            AddEventType(typeof(T).Name, typeof(T).AssemblyQualifiedName);
        }

        /// <summary>
        /// Add an name for an event type that represents IDictionary events.
        /// <para>
        /// Each entry in the type map is the property name and the fully-qualified
        /// class name or primitive type name.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        /// maps the name of each property in the Map event to the type
        /// (fully qualified classname) of its value in Map event instances.
        /// </param>
        public void AddEventType(string eventTypeName, Properties typeMap)
        {
            _mapNames.Put(eventTypeName, typeMap);
        }

        public void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap)
        {
            _nestableMapNames.Put(eventTypeName, new Dictionary<string, Object>(typeMap));
        }

        public void AddEventType(string eventTypeName, IDictionary<string, Object> typeMap, string[] superTypes)
        {
            _nestableMapNames.Put(eventTypeName, new Dictionary<string, Object>(typeMap));
            if (superTypes != null)
            {
                for (int i = 0; i < superTypes.Length; i++)
                {
                    AddMapSuperType(eventTypeName, superTypes[i]);
                }
            }
        }

        public void AddEventType(
            string eventTypeName,
            IDictionary<string, Object> typeMap,
            ConfigurationEventTypeMap mapConfig)
        {
            _nestableMapNames.Put(eventTypeName, new Dictionary<string, Object>(typeMap));
            _mapTypeConfigurations.Put(eventTypeName, mapConfig);
        }

        /// <summary>
        /// Add, for a given Map event type identified by the first parameter, the supertype (by its event type name).
        /// <para>
        /// Each Map event type may have any number of supertypes, each supertype must also be of a Map-type event.
        /// </para>
        /// </summary>
        /// <param name="mapeventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="mapSupertypeName">the name of a Map event type that is the supertype</param>
        public void AddMapSuperType(string mapeventTypeName, string mapSupertypeName)
        {
            ConfigurationEventTypeMap current = _mapTypeConfigurations.Get(mapeventTypeName);
            if (current == null)
            {
                current = new ConfigurationEventTypeMap();
                _mapTypeConfigurations.Put(mapeventTypeName, current);
            }
            ICollection<string> superTypes = current.SuperTypes;
            superTypes.Add(mapSupertypeName);
        }

        /// <summary>
        /// Add, for a given Object-array event type identified by the first parameter, the supertype (by its event type name).
        /// <para>
        /// Each Object array event type may have any number of supertypes, each supertype must also be of a Object-array-type event.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="supertypeName">the name of a Map event type that is the supertype</param>
        public void AddObjectArraySuperType(string eventTypeName, string supertypeName)
        {
            ConfigurationEventTypeObjectArray current = _objectArrayTypeConfigurations.Get(eventTypeName);
            if (current == null)
            {
                current = new ConfigurationEventTypeObjectArray();
                _objectArrayTypeConfigurations.Put(eventTypeName, current);
            }
            ICollection<string> superTypes = current.SuperTypes;
            if (!superTypes.IsEmpty())
            {
                throw new ConfigurationException("Object-array event types may not have multiple supertypes");
            }
            superTypes.Add(supertypeName);
        }

        /// <summary>
        /// Add configuration for a map event type.
        /// </summary>
        /// <param name="mapeventTypeName">configuration to add</param>
        /// <param name="config">map type configuration</param>
        public void AddMapConfiguration(string mapeventTypeName, ConfigurationEventTypeMap config)
        {
            _mapTypeConfigurations.Put(mapeventTypeName, config);
        }

        /// <summary>
        /// Add configuration for a object array event type.
        /// </summary>
        /// <param name="objectArrayeventTypeName">configuration to add</param>
        /// <param name="config">map type configuration</param>
        public void AddObjectArrayConfiguration(
            string objectArrayeventTypeName,
            ConfigurationEventTypeObjectArray config)
        {
            _objectArrayTypeConfigurations.Put(objectArrayeventTypeName, config);
        }

        /// <summary>
        /// Add an name for an event type that represents org.w3c.dom.Node events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="xmlDOMEventTypeDesc">descriptor containing property and mapping information for XML-DOM events</param>
        public void AddEventType(string eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc)
        {
            _eventTypesXmldom.Put(eventTypeName, xmlDOMEventTypeDesc);
        }

        public void AddEventType(string eventTypeName, string[] propertyNames, Object[] propertyTypes)
        {
            var propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(
                propertyNames, propertyTypes);
            _nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
        }

        public void AddEventType(
            string eventTypeName,
            string[] propertyNames,
            Object[] propertyTypes,
            ConfigurationEventTypeObjectArray config)
        {
            var propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(
                propertyNames, propertyTypes);
            _nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
            _objectArrayTypeConfigurations.Put(eventTypeName, config);
            if (config.SuperTypes?.Count > 1)
            {
                throw new ConfigurationException(ConfigurationEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
            }
        }

        public void AddRevisionEventType(
            string revisioneventTypeName,
            ConfigurationRevisionEventType revisionEventTypeConfig)
        {
            _revisionEventTypes.Put(revisioneventTypeName, revisionEventTypeConfig);
        }

        /// <summary>
        /// Add a database reference with a given database name.
        /// </summary>
        /// <param name="name">is the database name</param>
        /// <param name="configurationDBRef">descriptor containing database connection and access policy information</param>
        public void AddDatabaseReference(string name, ConfigurationDBRef configurationDBRef)
        {
            _databaseReferences.Put(name, configurationDBRef);
        }

        public void AddEventType<T>(ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(typeof(T).Name, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        public void AddEventType<T>(string eventTypeName, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(eventTypeName, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        /// <summary>
        /// Add an name for an event type that represents legacy object type events.
        /// Note that when adding multiple names for the same class the names represent an
        /// alias to the same event type since event type identity for classes is per class.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">fully-qualified class name of the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        public void AddEventType(
            string eventTypeName,
            string eventClass,
            ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            eventClass = TypeHelper.TryResolveAbsoluteTypeName(eventClass);
            _eventClasses.Put(eventTypeName, eventClass);
            _eventTypesLegacy.Put(eventTypeName, legacyEventTypeDesc);
        }

        public void AddEventType(
            string eventTypeName,
            Type eventClass,
            ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(eventTypeName, eventClass.AssemblyQualifiedName, legacyEventTypeDesc);
        }

        /// <summary>
        /// Add a namespace. Adding will suppress the use of the default namespaces.
        /// </summary>
        /// <param name="importName">is a fully-qualified class name or a package name with wildcard</param>
        /// <throws>
        /// ConfigurationException if incorrect package or class names are encountered
        /// </throws>
        public void AddImport(string importName)
        {
            string[] importParts = importName.Split(',');
            if (importParts.Length == 1)
            {
                AddImport(importName, null);
            }
            else
            {
                AddImport(importParts[0], importName.Substring(importParts[0].Length + 1).TrimStart());
            }
        }

        /// <summary>
        /// Adds the class or namespace (importName) ot the list of automatically imported types.
        /// </summary>
        /// <param name="importName">Name of the import.</param>
        /// <param name="assemblyNameOrFile">The assembly name or file.</param>
        public void AddImport(String importName, String assemblyNameOrFile)
        {
            _imports.Add(new AutoImportDesc(importName, assemblyNameOrFile));
        }

        /// <summary>
        /// Adds an import for a specific type.
        /// </summary>
        /// <param name="importClass">The auto import.</param>
        public void AddImport(Type importClass)
        {
            AddImport(importClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Adds the import.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddImport<T>()
        {
            AddImport(typeof(T));
        }

        /// <summary>
        /// Adds an import for the namespace associated with the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddNamespaceImport<T>()
        {
            AddImport(typeof(T).Namespace);
        }

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="importName">Name of the import.</param>
        /// <param name="assemblyNameOrFile">The assembly name or file.</param>
        public void AddAnnotationImport(String importName, String assemblyNameOrFile)
        {
            _annotationImports.Add(new AutoImportDesc(importName, assemblyNameOrFile));
        }

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="autoImport">The automatic import.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void AddAnnotationImport(string autoImport)
        {
            string[] importParts = autoImport.Split(',');
            if (importParts.Length == 1)
            {
                AddAnnotationImport(autoImport, null);
            }
            else
            {
                AddAnnotationImport(importParts[0], importParts[1]);
            }
        }

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="autoImport">The automatic import.</param>
        public void AddAnnotationImport(Type autoImport)
        {
            AddAnnotationImport(autoImport.FullName, autoImport.Assembly.FullName);
        }

        /// <summary>
        /// Adds the annotation import.
        /// </summary>
        /// <param name="importNamespace"></param>
        /// <typeparam name="T"></typeparam>
        public void AddAnnotationImport<T>(bool importNamespace = false)
        {
            if (importNamespace)
            {
                AddAnnotationImport(typeof(T).Namespace, typeof(T).Assembly.FullName);
            }
            else
            {
                AddAnnotationImport(typeof(T).FullName, typeof(T).Assembly.FullName);
            }
        }

        /// <summary>
        /// Remove an import.
        /// </summary>
        /// <param name="autoImportDesc"></param>
        public void RemoveImport(AutoImportDesc autoImportDesc)
        {
            _imports.Remove(autoImportDesc);
        }

        /// <summary>
        /// Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="className">is the class name (simple or fully-qualified) providing methods</param>
        /// <param name="methodInvocationConfig">is the cache configuration</param>
        public void AddMethodRef(string className, ConfigurationMethodRef methodInvocationConfig)
        {
            _methodInvocationReferences.Put(className, methodInvocationConfig);
        }

        /// <summary>
        /// Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="clazz">is the class providing methods</param>
        /// <param name="methodInvocationConfig">is the cache configuration</param>
        public void AddMethodRef(Type clazz, ConfigurationMethodRef methodInvocationConfig)
        {
            _methodInvocationReferences.Put(clazz.FullName, methodInvocationConfig);
        }

        public IDictionary<string, string> EventTypeNames => _eventClasses;
        public IDictionary<string, Properties> EventTypesMapEvents => _mapNames;
        public IDictionary<string, IDictionary<string, object>> EventTypesNestableMapEvents => _nestableMapNames;
        public IDictionary<string, IDictionary<string, object>> EventTypesNestableObjectArrayEvents => _nestableObjectArrayNames;
        public IDictionary<string, ConfigurationEventTypeXMLDOM> EventTypesXMLDOM => _eventTypesXmldom;
        public IDictionary<string, ConfigurationEventTypeAvro> EventTypesAvro => _eventTypesAvro;
        public IDictionary<string, ConfigurationEventTypeLegacy> EventTypesLegacy => _eventTypesLegacy;
        public IList<AutoImportDesc> Imports => _imports;
        public IList<AutoImportDesc> AnnotationImports => _annotationImports;
        public IDictionary<string, ConfigurationDBRef> DatabaseReferences => _databaseReferences;
        public IList<ConfigurationPlugInView> PlugInViews => _plugInViews;
        public IDictionary<string, ConfigurationEventTypeObjectArray> ObjectArrayTypeConfigurations => _objectArrayTypeConfigurations;
        public IList<ConfigurationPlugInVirtualDataWindow> PlugInVirtualDataWindows => _plugInVirtualDataWindows;
        public IList<ConfigurationPluginLoader> PluginLoaders => _pluginLoaders;
        public IList<ConfigurationPlugInAggregationFunction> PlugInAggregationFunctions => _plugInAggregationFunctions;
        public IList<ConfigurationPlugInAggregationMultiFunction> PlugInAggregationMultiFunctions => _plugInAggregationMultiFunctions;
        public IList<ConfigurationPlugInSingleRowFunction> PlugInSingleRowFunctions => _plugInSingleRowFunctions;
        public IList<ConfigurationPlugInPatternObject> PlugInPatternObjects => _plugInPatternObjects;
        public IDictionary<string, ConfigurationVariable> Variables => _variables;
        public IDictionary<string, ConfigurationMethodRef> MethodInvocationReferences => _methodInvocationReferences;
        public IDictionary<string, ConfigurationRevisionEventType> RevisionEventTypes => _revisionEventTypes;
        public IDictionary<string, ConfigurationEventTypeMap> MapTypeConfigurations => _mapTypeConfigurations;

        /// <summary>
        /// Add a plugin loader (f.e. an input/output adapter loader).
        /// <para>
        /// The class is expected to implement <seealso cref="com.espertech.esper.plugin.PluginLoader" />.
        /// </para>
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        /// <param name="configuration">is loader cofiguration entries</param>
        public void AddPluginLoader(string loaderName, string className, Properties configuration)
        {
            AddPluginLoader(loaderName, className, configuration, null);
        }

        /// <summary>
        /// Add a plugin loader (f.e. an input/output adapter loader) without any additional loader configuration
        /// <para>
        /// The class is expected to implement <seealso cref="com.espertech.esper.plugin.PluginLoader" />.
        /// </para>
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        public void AddPluginLoader(string loaderName, string className)
        {
            AddPluginLoader(loaderName, className, null, null);
        }

        /// <summary>
        /// Add a plugin loader (f.e. an input/output adapter loader).
        /// <para>
        /// The class is expected to implement <seealso cref="com.espertech.esper.plugin.PluginLoader" />.
        /// </para>
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
            var pluginLoader = new ConfigurationPluginLoader();
            pluginLoader.LoaderName = loaderName;
            pluginLoader.TypeName = className;
            pluginLoader.ConfigProperties = configuration;
            pluginLoader.ConfigurationXML = configurationXML;
            _pluginLoaders.Add(pluginLoader);
        }

        /// <summary>
        /// Add a view for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the view should be available under</param>
        /// <param name="name">is the name of the view</param>
        /// <param name="viewFactoryClass">is the view factory class to use</param>
        public void AddPlugInView(string @namespace, string name, string viewFactoryClass)
        {
            var configurationPlugInView = new ConfigurationPlugInView();
            configurationPlugInView.Namespace = @namespace;
            configurationPlugInView.Name = name;
            configurationPlugInView.FactoryClassName = viewFactoryClass;
            _plugInViews.Add(configurationPlugInView);
        }

        public void AddPlugInView(string @namespace, string name, Type viewFactoryClass)
        {
            AddPlugInView(@namespace, name, viewFactoryClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Add a virtual data window for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="factoryClass">is the view factory class to use</param>
        public void AddPlugInVirtualDataWindow(string @namespace, string name, string factoryClass)
        {
            AddPlugInVirtualDataWindow(@namespace, name, factoryClass, null);
        }

        /// <summary>
        /// Add a virtual data window for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="factoryClass">is the view factory class to use</param>
        /// <param name="customConfigurationObject">additional configuration to be passed along</param>
        public void AddPlugInVirtualDataWindow(
            string @namespace,
            string name,
            string factoryClass,
            object customConfigurationObject)
        {
            var configurationPlugInVirtualDataWindow = new ConfigurationPlugInVirtualDataWindow();
            configurationPlugInVirtualDataWindow.Namespace = @namespace;
            configurationPlugInVirtualDataWindow.Name = name;
            configurationPlugInVirtualDataWindow.FactoryClassName = factoryClass;
            configurationPlugInVirtualDataWindow.Config = customConfigurationObject;
            _plugInVirtualDataWindows.Add(configurationPlugInVirtualDataWindow);
        }

        /// <summary>
        /// Add a pattern event observer for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the observer should be available under</param>
        /// <param name="name">is the name of the observer</param>
        /// <param name="observerFactoryClass">is the observer factory class to use</param>
        public void AddPlugInPatternObserver(string @namespace, string name, string observerFactoryClass)
        {
            var entry = new ConfigurationPlugInPatternObject();
            entry.Namespace = @namespace;
            entry.Name = name;
            entry.FactoryClassName = observerFactoryClass;
            entry.PatternObjectType = ConfigurationPlugInPatternObject.PatternObjectTypeEnum.OBSERVER;
            _plugInPatternObjects.Add(entry);
        }

        /// <summary>
        /// Add a pattern guard for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the guard should be available under</param>
        /// <param name="name">is the name of the guard</param>
        /// <param name="guardFactoryClass">is the guard factory class to use</param>
        public void AddPlugInPatternGuard(string @namespace, string name, Type guardFactoryClass)
        {
            AddPlugInPatternGuard(@namespace, name, guardFactoryClass.AssemblyQualifiedName);
        }

        /// <summary>
        /// Add a pattern guard for plug-in.
        /// </summary>
        /// <param name="namespace">is the namespace the guard should be available under</param>
        /// <param name="name">is the name of the guard</param>
        /// <param name="guardFactoryClass">is the guard factory class to use</param>
        public void AddPlugInPatternGuard(string @namespace, string name, string guardFactoryClass)
        {
            var entry = new ConfigurationPlugInPatternObject();
            entry.Namespace = @namespace;
            entry.Name = name;
            entry.FactoryClassName = guardFactoryClass;
            entry.PatternObjectType = ConfigurationPlugInPatternObject.PatternObjectTypeEnum.GUARD;
            _plugInPatternObjects.Add(entry);
        }

        public void AddEventTypeAutoName(string @namespace)
        {
            _eventTypeAutoNamePackages.Add(@namespace);
        }

        public void AddVariable<TValue>(string variableName, TValue initializationValue)
        {
            AddVariable(variableName, typeof(TValue).FullName, initializationValue, false);
        }

        public void AddVariable(string variableName, Type type, Object initializationValue)
        {
            AddVariable(variableName, type.FullName, initializationValue, false);
        }

        /// <summary>
        /// Add variable that can be a constant.
        /// </summary>
        /// <param name="variableName">name of variable</param>
        /// <param name="type">variable type</param>
        /// <param name="initializationValue">initial value</param>
        /// <param name="constant">constant indicator</param>
        public void AddVariable(string variableName, Type type, Object initializationValue, bool constant)
        {
            AddVariable(variableName, type.FullName, initializationValue, constant);
        }

        public void AddVariable(string variableName, string type, Object initializationValue)
        {
            AddVariable(variableName, type, initializationValue, false);
        }

        public void AddVariable(string variableName, string type, Object initializationValue, bool constant)
        {
            var configVar = new ConfigurationVariable();
            configVar.VariableType = type;
            configVar.InitializationValue = initializationValue;
            configVar.IsConstant = constant;
            _variables.Put(variableName, configVar);
        }

        /// <summary>
        /// Adds an event representation responsible for creating event types (event metadata) and event bean instances (events) for
        /// a certain kind of object representation that holds the event property values.
        /// </summary>
        /// <param name="eventRepresentationRootUri">
        /// uniquely identifies the event representation and acts as a parent
        /// for child Uris used in resolving
        /// </param>
        /// <param name="eventRepresentationClassName">is the name of the class implementing <seealso cref="com.espertech.esper.plugin.PlugInEventRepresentation" />.</param>
        /// <param name="initializer">is optional configuration or initialization information, or null if none required</param>
        public void AddPlugInEventRepresentation(
            Uri eventRepresentationRootUri,
            string eventRepresentationClassName,
            object initializer)
        {
            var config = new ConfigurationPlugInEventRepresentation();
            config.EventRepresentationTypeName = eventRepresentationClassName;
            config.Initializer = initializer;
            _plugInEventRepresentation.Put(eventRepresentationRootUri, config);
        }

        /// <summary>
        /// Adds an event representation responsible for creating event types (event metadata) and event bean instances (events) for
        /// a certain kind of object representation that holds the event property values.
        /// </summary>
        /// <param name="eventRepresentationRootUri">
        /// uniquely identifies the event representation and acts as a parent
        /// for child Uris used in resolving
        /// </param>
        /// <param name="eventRepresentationClass">is the class implementing <seealso cref="com.espertech.esper.plugin.PlugInEventRepresentation" />.</param>
        /// <param name="initializer">is optional configuration or initialization information, or null if none required</param>
        public void AddPlugInEventRepresentation(
            Uri eventRepresentationRootUri,
            Type eventRepresentationClass,
            object initializer)
        {
            AddPlugInEventRepresentation(
                eventRepresentationRootUri,
                eventRepresentationClass.AssemblyQualifiedName,
                initializer);
        }

        public void AddPlugInEventType(string eventTypeName, Uri[] resolutionURIs, object initializer)
        {
            var config = new ConfigurationPlugInEventType();
            config.EventRepresentationResolutionURIs = resolutionURIs;
            config.Initializer = initializer;
            _plugInEventTypes.Put(eventTypeName, config);
        }

        public IList<Uri> PlugInEventTypeResolutionURIs
        {
            get => _plugInEventTypeResolutionUris;
            set => _plugInEventTypeResolutionUris = value;
        }

        public IDictionary<Uri, ConfigurationPlugInEventRepresentation> PlugInEventRepresentation => _plugInEventRepresentation;
        public IDictionary<string, ConfigurationPlugInEventType> PlugInEventTypes => _plugInEventTypes;
        public ISet<string> EventTypeAutoNamePackages => _eventTypeAutoNamePackages;
        public ConfigurationEngineDefaults EngineDefaults => _engineDefaults;

        public void AddVariantStream(string variantStreamName, ConfigurationVariantStream variantStreamConfig)
        {
            _variantStreams.Put(variantStreamName, variantStreamConfig);
        }

        public IDictionary<string, ConfigurationVariantStream> VariantStreams => _variantStreams;

        public void UpdateMapEventType(string mapeventTypeName, IDictionary<string, Object> typeMap)
        {
            throw new UnsupportedOperationException("Map type update is only available in runtime configuration");
        }

        public void UpdateObjectArrayEventType(string myEvent, string[] namesNew, Object[] typesNew)
        {
            throw new UnsupportedOperationException(
                "Object-array type update is only available in runtime configuration");
        }

        public void ReplaceXMLEventType(string xmlEventTypeName, ConfigurationEventTypeXMLDOM config)
        {
            throw new UnsupportedOperationException("XML type update is only available in runtime configuration");
        }

        public ICollection<string> GetEventTypeNameUsedBy(string eventTypeName)
        {
            throw new UnsupportedOperationException("Get event type by name is only available in runtime configuration");
        }

        public bool IsVariantStreamExists(string name)
        {
            return _variantStreams.ContainsKey(name);
        }

        public void SetMetricsReportingInterval(string stmtGroupName, long newIntervalMSec)
        {
            EngineDefaults.MetricsReporting.SetStatementGroupInterval(stmtGroupName, newIntervalMSec);
        }

        public void SetMetricsReportingStmtEnabled(string statementName)
        {
            throw new UnsupportedOperationException(
                "Statement metric reporting can only be enabled or disabled at runtime");
        }

        public void SetMetricsReportingStmtDisabled(string statementName)
        {
            throw new UnsupportedOperationException(
                "Statement metric reporting can only be enabled or disabled at runtime");
        }

        public EventType GetEventType(string eventTypeName)
        {
            throw new UnsupportedOperationException("Obtaining an event type by name is only available at runtime");
        }

        public ICollection<EventType> EventTypes => throw new UnsupportedOperationException("Obtaining event types is only available at runtime");

        public void SetMetricsReportingEnabled()
        {
            EngineDefaults.MetricsReporting.IsEnableMetricsReporting = true;
        }

        public void SetMetricsReportingDisabled()
        {
            EngineDefaults.MetricsReporting.IsEnableMetricsReporting = false;
        }

        public long PatternMaxSubexpressions
        {
            set => EngineDefaults.Patterns.MaxSubexpressions = value;
        }

        public long? MatchRecognizeMaxStates
        {
            set => EngineDefaults.MatchRecognize.MaxStates = value;
        }

        /// <summary>
        /// Use the configuration specified in an application
        /// resource named <tt>esper.cfg.xml</tt>.
        /// </summary>
        /// <exception cref="EPException">thrown to indicate error reading configuration</exception>
        /// <returns>Configuration initialized from the resource</returns>
        public Configuration Configure()
        {
            Configure('/' + ESPER_DEFAULT_CONFIG);
            return this;
        }

        /// <summary>
        /// Use the configuration specified in the given application
        /// resource. The format of the resource is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// <para>
        /// The resource is found via <tt>GetConfigurationInputStream(resource)</tt>.
        /// That method can be overridden to implement an arbitrary lookup strategy.
        /// </para>
        /// <para>
        /// See <tt>getResourceAsStream</tt> for information on how the resource name is resolved.
        /// </para>
        /// </summary>
        /// <param name="resource">if the file name of the resource</param>
        /// <exception cref="EPException">thrown to indicate error reading configuration</exception>
        /// <returns>Configuration initialized from the resource</returns>
        public Configuration Configure(string resource)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Configuring from resource: " + resource);
            }
            var stream = GetConfigurationInputStream(resource);
            _container.Resolve<IConfigurationParser>()
                .DoConfigure(this, stream, resource);
            return this;
        }

        /// <summary>
        /// Use the configuration specified by the given URL.
        /// The format of the document obtained from the URL is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="url">URL from which you wish to load the configuration</param>
        /// <exception cref="EPException">is thrown when the URL could not be access</exception>
        /// <returns>A configuration configured via the file</returns>
        public Configuration Configure(Uri url)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("configuring from url: " + url);
            }
            try
            {
                _container.Resolve<IConfigurationParser>()
                    .DoConfigure(this, WebRequest.Create(url).GetResponse().GetResponseStream(), url.ToString());
                return this;
            }
            catch (IOException ioe)
            {
                throw new EPException("could not configure from URL: " + url, ioe);
            }
        }

        /// <summary>
        /// Use the configuration specified in the given application
        /// file. The format of the file is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="configFile"><tt>File</tt> from which you wish to load the configuration</param>
        /// <exception cref="EPException">when the file could not be found</exception>
        /// <returns>A configuration configured via the file</returns>
        public Configuration Configure(FileInfo configFile)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("configuring from file: " + configFile.Name);
            }
            try
            {
                using (var stream = new FileStream(configFile.FullName, FileMode.Open, FileAccess.Read)) {
                    _container.Resolve<IConfigurationParser>()
                        .DoConfigure(this, stream, configFile.ToString());
                }
            }
            catch (FileNotFoundException fnfe)
            {
                throw new EPException("could not find file: " + configFile, fnfe);
            }
            return this;
        }

        public bool RemoveEventType(string eventTypeName, bool force)
        {
            _eventClasses.Remove(eventTypeName);
            _eventTypesXmldom.Remove(eventTypeName);
            _eventTypesAvro.Remove(eventTypeName);
            _eventTypesLegacy.Remove(eventTypeName);
            _mapNames.Remove(eventTypeName);
            _nestableMapNames.Remove(eventTypeName);
            _mapTypeConfigurations.Remove(eventTypeName);
            _plugInEventTypes.Remove(eventTypeName);
            _revisionEventTypes.Remove(eventTypeName);
            _variantStreams.Remove(eventTypeName);
            return true;
        }

        public ICollection<string> GetVariableNameUsedBy(string variableName)
        {
            throw new UnsupportedOperationException(
                "Get variable use information is only available in runtime configuration");
        }

        public bool RemoveVariable(string name, bool force)
        {
            return _variables.Remove(name);
        }

        public void AddEventTypeAvro(string eventTypeName, ConfigurationEventTypeAvro avro)
        {
            _eventTypesAvro.Put(eventTypeName, avro);
        }

        /// <summary>
        /// Use the mappings and properties specified in the given XML document.
        /// The format of the file is defined in
        /// <tt>esper-configuration-(version).xsd</tt>.
        /// </summary>
        /// <param name="document">an XML document from which you wish to load the configuration</param>
        /// <returns>
        /// A configuration configured via the <tt>Document</tt>
        /// </returns>
        /// <exception cref="EPException">if there is problem in accessing the document.</exception>
        public Configuration Configure(XmlDocument document)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("configuring from XML document");
            }
            _container.Resolve<IConfigurationParser>().DoConfigure(this, document);
            return this;
        }

        /// <summary>
        /// Reset to an empty configuration.
        /// </summary>
        protected void Reset()
        {
            _eventClasses = new Dictionary<string, string>();
            _mapNames = new Dictionary<string, Properties>();
            _nestableMapNames = new Dictionary<string, IDictionary<string, Object>>();
            _nestableObjectArrayNames = new Dictionary<string, IDictionary<string, Object>>();
            _eventTypesXmldom = new Dictionary<string, ConfigurationEventTypeXMLDOM>();
            _eventTypesAvro = new Dictionary<string, ConfigurationEventTypeAvro>();
            _eventTypesLegacy = new Dictionary<string, ConfigurationEventTypeLegacy>();
            _databaseReferences = new Dictionary<string, ConfigurationDBRef>();
            _imports = new List<AutoImportDesc>();
            _annotationImports = new List<AutoImportDesc>();
            AddDefaultImports();
            _plugInViews = new List<ConfigurationPlugInView>();
            _plugInVirtualDataWindows = new List<ConfigurationPlugInVirtualDataWindow>();
            _pluginLoaders = new List<ConfigurationPluginLoader>();
            _plugInAggregationFunctions = new List<ConfigurationPlugInAggregationFunction>();
            _plugInAggregationMultiFunctions = new List<ConfigurationPlugInAggregationMultiFunction>();
            _plugInSingleRowFunctions = new List<ConfigurationPlugInSingleRowFunction>();
            _plugInPatternObjects = new List<ConfigurationPlugInPatternObject>();
            _engineDefaults = new ConfigurationEngineDefaults();
            _eventTypeAutoNamePackages = new FIFOHashSet<string>();
            _variables = new Dictionary<string, ConfigurationVariable>();
            _methodInvocationReferences = new Dictionary<string, ConfigurationMethodRef>();
            _plugInEventRepresentation = new Dictionary<Uri, ConfigurationPlugInEventRepresentation>();
            _plugInEventTypes = new Dictionary<string, ConfigurationPlugInEventType>();
            _revisionEventTypes = new Dictionary<string, ConfigurationRevisionEventType>();
            _variantStreams = new Dictionary<string, ConfigurationVariantStream>();
            _mapTypeConfigurations = new Dictionary<string, ConfigurationEventTypeMap>();
            _objectArrayTypeConfigurations = new Dictionary<string, ConfigurationEventTypeObjectArray>();
            _transientConfiguration = new Dictionary<string, object>();
        }

        /// <summary>
        /// Use these imports until the user specifies something else.
        /// </summary>
        private void AddDefaultImports()
        {
            _imports.Add(new AutoImportDesc("System"));
            _imports.Add(new AutoImportDesc("System.Collections"));
            _imports.Add(new AutoImportDesc("System.Text"));
            _imports.Add(new AutoImportDesc(ANNOTATION_IMPORT));
            _imports.Add(new AutoImportDesc(typeof(BeaconSource).Namespace, (string) null));
        }
    }

    public static class ConfigurationExtensions
    {
        public static void AddPlugInVirtualDataWindow(
            this Configuration configuration,
            string @namespace,
            string name,
            Type factoryClass)
        {
            configuration.AddPlugInVirtualDataWindow(
                @namespace, name, factoryClass.AssemblyQualifiedName);
        }

        public static void AddPlugInVirtualDataWindow(
            this Configuration configuration,
            string @namespace,
            string name,
            Type factoryClass,
            object customConfigurationObject)
        {
            configuration.AddPlugInVirtualDataWindow(
                @namespace, name, factoryClass.AssemblyQualifiedName, customConfigurationObject);
        }
    }
} // end of namespace