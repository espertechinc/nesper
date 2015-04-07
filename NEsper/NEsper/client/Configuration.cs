///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.dataflow.ops;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.client
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// An instance of <tt>Configuration</tt> allows the application
    /// to specify properties to be used when
    /// creating a <tt>EPServiceProvider</tt>. Usually an application will create
    /// a single <tt>Configuration</tt>, then get one or more instances of
    /// <see cref="EPServiceProvider" /> via <see cref="EPServiceProviderManager" />.
    /// The <tt>Configuration</tt> is meant
    /// only as an initialization-time object. <tt>EPServiceProvider</tt>s are
    /// immutable and do not retain any association back to the
    /// <tt>Configuration</tt>.
    /// <para>
    /// The format of an Esper XML configuration file is defined in
    /// <tt>esper-configuration-x.y.xsd</tt>.
    /// </para>
    /// </summary>
    [Serializable]
    public class Configuration
        : ConfigurationOperations
        , ConfigurationInformation
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Import name of the package that hosts the annotation classes.</summary>
        public readonly static String ANNOTATION_IMPORT = typeof (NameAttribute).Namespace;

        /// <summary> Default name of the configuration file.</summary>
        internal const String ESPER_DEFAULT_CONFIG = "esper.cfg.xml";

        /// <summary> Map of event name and fully-qualified type name.</summary>
        private IDictionary<String, String> _eventClasses;

        /// <summary> Map of event type name and XML DOM configuration.</summary>
        private IDictionary<String, ConfigurationEventTypeXMLDOM> _eventTypesXmldom;

        /// <summary> Map of event type name and Legacy-type event configuration.</summary>
        private IDictionary<String, ConfigurationEventTypeLegacy> _eventTypesLegacy;

        /// <summary>
        /// The type aliases for events that are backed by Map, not containing
        /// strongly-typed nested maps.
        /// </summary>
        private IDictionary<String, Properties> _mapNames;

        /// <summary>
        /// The type aliases for events that are backed by Map, possibly containing
        /// strongly-typed nested maps.
        /// <para/>
        /// Each entries value must be either a Class or a DataMap to define nested maps.
        /// </summary>
        private IDictionary<String, IDictionary<String, Object>> _nestableMapNames;

        /// <summary>
        /// The type names for events that are backed by java.util.Map,
        /// possibly containing strongly-typed nested maps.
        /// <para>
        /// Each entries value must be either a Class or a Map to
        /// define nested maps.
        /// </para>
        /// </summary>
        private IDictionary<String, IDictionary<String, Object>> _nestableObjectArrayNames;

        /// <summary>Map event types additional configuration information.</summary>
        private IDictionary<String, ConfigurationEventTypeMap> _mapTypeConfigurations;

        /// <summary>
        /// Map event types additional configuration information.
        /// </summary>
        private IDictionary<String, ConfigurationEventTypeObjectArray> _objectArrayTypeConfigurations;

        /// <summary>
        /// The class and namespace imports that will be used to resolve partial class names.
        /// </summary>

        private IList<AutoImportDesc> _imports;

        private IDictionary<String, ConfigurationDBRef> _databaseReferences;

        /// <summary>List of configured plug-in views.</summary>
        private IList<ConfigurationPlugInView> _plugInViews;

        /// <summary>List of configured plug-in views.</summary>
        private IList<ConfigurationPlugInVirtualDataWindow> _plugInVirtualDataWindows;

        /// <summary>List of configured plug-in pattern objects.</summary>
        private List<ConfigurationPlugInPatternObject> _plugInPatternObjects;

        /// <summary>List of configured plug-in aggregation functions.</summary>
        private IList<ConfigurationPlugInAggregationFunction> _plugInAggregationFunctions;

        /// <summary>List of configured plug-in aggregation multi-functions.</summary>
        protected IList<ConfigurationPlugInAggregationMultiFunction> _plugInAggregationMultiFunctions;

        /// <summary>List of configured plug-in single-row functions.</summary>
        private IList<ConfigurationPlugInSingleRowFunction> _plugInSingleRowFunctions;

        /// <summary>List of adapter loaders.</summary>
        private IList<ConfigurationPluginLoader> _pluginLoaders;

        /// <summary>
        /// Saves engine default configs such as threading settings
        /// </summary>
        private ConfigurationEngineDefaults _engineDefaults;

        /// <summary>
        /// Saves the namespaces to search to resolve event type aliases.
        /// </summary>
        private ICollection<String> _eventTypeAutoNamePackages;

        /// <summary>
        /// Map of variables.
        /// </summary>
        private IDictionary<String, ConfigurationVariable> _variables;

        /// <summary>
        /// Map of class name and configuration for method invocations on that class.
        /// </summary>
        private IDictionary<String, ConfigurationMethodRef> _methodInvocationReferences;

        /// <summary>Map of plug-in event representation name and configuration</summary>
        private IDictionary<Uri, ConfigurationPlugInEventRepresentation> _plugInEventRepresentation;

        /// <summary>Map of plug-in event types.</summary>
        private IDictionary<String, ConfigurationPlugInEventType> _plugInEventTypes;

        /// <summary>All revision event types which allow updates to past events.</summary>
        private IDictionary<String, ConfigurationRevisionEventType> _revisionEventTypes;

        /// <summary>Variant streams allow events of disparate types to be treated the same.</summary>
        private IDictionary<String, ConfigurationVariantStream> _variantStreams;

        /// <summary>
        /// Constructs an empty configuration. The auto import values
        /// are set to System, System.Collections and System.Text
        /// </summary>

        public Configuration()
        {
            Reset();
        }

        /// <summary>
        /// Gets or sets the service context factory type name
        /// </summary>
        /// <value></value>
        /// <returns>class name</returns>
        public string EPServicesContextFactoryClassName { get; set; }

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryClassName">is the fully-qualified class name of the class implementing the aggregation function factory interface <seealso cref="AggregationFunctionFactory" /></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionFactory(String functionName, String aggregationFactoryClassName)
        {
            var entry = new ConfigurationPlugInAggregationFunction
            {
                Name = functionName,
                FactoryClassName = aggregationFactoryClassName
            };
            _plugInAggregationFunctions.Add(entry);
        }

        /// <summary>
        /// Adds a plug-in aggregation function given a EPL function name and an aggregation factory class name.
        /// <para />
        /// The same function name cannot be added twice.
        /// </summary>
        /// <param name="functionName">is the new aggregation function name for use in EPL</param>
        /// <param name="aggregationFactoryType">Type of the aggregation factory.  Must implement <seealso cref="AggregationFunctionFactory"/></param>
        /// <throws>ConfigurationException is thrown to indicate a problem adding the aggregation function</throws>
        public void AddPlugInAggregationFunctionFactory(String functionName, Type aggregationFactoryType)
        {
            AddPlugInAggregationFunctionFactory(functionName, aggregationFactoryType.AssemblyQualifiedName);
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

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName)
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, ValueCache.DISABLED);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName, ValueCache valueCache)
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, valueCache, FilterOptimizable.ENABLED);
        }

        public void AddPlugInSingleRowFunction(String functionName, String className, String methodName, FilterOptimizable filterOptimizable) 
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, ValueCache.DISABLED, filterOptimizable);
        }

        /// <summary>Add single-row function with configurations. </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <throws>ConfigurationException thrown to indicate that the configuration is invalid</throws>

        public void AddPlugInSingleRowFunction(String functionName,
                                               String className,
                                               String methodName,
                                               ValueCache valueCache,
                                               FilterOptimizable filterOptimizable)
        {
            AddPlugInSingleRowFunction(functionName, className, methodName, valueCache, filterOptimizable, false);
        }

        /// <summary>
        /// Add single-row function with configurations.
        /// </summary>
        /// <param name="functionName">EPL name of function</param>
        /// <param name="className">providing fully-qualified class name</param>
        /// <param name="methodName">providing method name</param>
        /// <param name="valueCache">value cache settings</param>
        /// <param name="filterOptimizable">settings whether subject to optimizations</param>
        /// <param name="rethrowExceptions">if set to <c>true</c> [rethrow exceptions].</param>
        /// <throws>ConfigurationException thrown to indicate that the configuration is invalid</throws>
        public void AddPlugInSingleRowFunction(String functionName,
                                               String className,
                                               String methodName,
                                               ValueCache valueCache,
                                               FilterOptimizable filterOptimizable,
                                               bool rethrowExceptions)
        {
            var entry = new ConfigurationPlugInSingleRowFunction();
            entry.FunctionClassName = className;
            entry.FunctionMethodName = methodName;
            entry.Name = functionName;
            entry.ValueCache = valueCache;
            entry.FilterOptimizable = filterOptimizable;
            entry.RethrowExceptions = rethrowExceptions;
            _plugInSingleRowFunctions.Add(entry);
        }

        /// <summary>
        /// Add a database reference with a given database name.
        /// </summary>
        /// <param name="name">is the database name</param>
        /// <param name="configurationDBRef">descriptor containing database connection and access policy information</param>
        public virtual void AddDatabaseReference(String name, ConfigurationDBRef configurationDBRef)
        {
            _databaseReferences[name] = configurationDBRef;
        }

        /// <summary>
        /// Checks if an eventTypeName has already been registered for that name.
        /// </summary>
        /// <param name="eventTypeName">the name</param>
        /// <returns>true if already registered</returns>

        public bool IsEventTypeExists(String eventTypeName)
        {
            return _eventClasses.ContainsKey(eventTypeName)
                    || _mapNames.ContainsKey(eventTypeName)
                    || _nestableMapNames.ContainsKey(eventTypeName)
                    || _nestableObjectArrayNames.ContainsKey(eventTypeName)
                    || _eventTypesXmldom.ContainsKey(eventTypeName);
            //note: no need to check legacy as they get added as class event type
        }

        /// <summary>
        /// Add a name for an event type represented by plain-old object events.
        /// <para>
        /// Note that when adding multiple names for the same type the names represent an
        /// alias to the same event type since event type identity for types is per type.
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="nativeEventTypeName">fully-qualified class name of the event type</param>

        public virtual void AddEventType(String eventTypeName, String nativeEventTypeName)
        {
            _eventClasses[eventTypeName] = nativeEventTypeName;
        }

        /// <summary>
        /// Add a name for an event type represented by plain-old object events.
        /// Note that when adding multiple names for the same type the names represent an
        /// alias to the same event type since event type identity for types is per type.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventType">is the event class for which to create the name</param>
        /// <throws>
        /// ConfigurationException if the name is already in used for a different type
        /// </throws>
        public virtual void AddEventType(String eventTypeName, Type eventType)
        {
            AddEventType(eventTypeName, eventType.AssemblyQualifiedName);
        }

        /// <summary>
        /// Add a name for an event type represented by plain-old object events,
        /// and the name is the simple name of the type.
        /// </summary>
        /// <param name="eventType">the event type for which to create the name</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        public void AddEventType(Type eventType)
        {
            AddEventType(eventType.Name, eventType.AssemblyQualifiedName);
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
        /// Add an name for an event type that represents map events.
        /// <para/>
        /// Each entry in the type map is the property name and the fully-qualified type name or primitive type name.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">maps the name of each property in the Map event to the type(fully qualified classname) of its value in Map event instances. </param>
        public void AddEventType(String eventTypeName, Properties typeMap)
        {
            _mapNames.Put(eventTypeName, typeMap);
        }

        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap)
        {
            _nestableMapNames.Put(eventTypeName, CopyAndBoxTypeMap(typeMap));
        }

        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, String[] superTypes)
        {
            _nestableMapNames.Put(eventTypeName, new LinkedHashMap<string, object>(typeMap));
            if (superTypes != null)
            {
                for (int i = 0; i < superTypes.Length; i++)
                {
                    AddMapSuperType(eventTypeName, superTypes[i]);
                }
            }
        }

        public void AddEventType(String eventTypeName, IDictionary<String, Object> typeMap, ConfigurationEventTypeMap mapConfig)
        {
            _nestableMapNames.Put(eventTypeName, CopyAndBoxTypeMap(typeMap));
            _mapTypeConfigurations.Put(eventTypeName, mapConfig);
        }

        private static LinkedHashMap<string, object> CopyAndBoxTypeMap(IDictionary<String, Object> typeMap)
        {
            var trueTypeMap = new LinkedHashMap<string, object>();
            foreach (var typeMapEntry in typeMap)
            {
                var value = typeMapEntry.Value;
                if (value is Type)
                    value = value.GetBoxedType();

                trueTypeMap[typeMapEntry.Key] = value;
            }
            return trueTypeMap;
        }

        /// <summary>
        /// Add, for a given Map event type identified by the first parameter, the supertype (by its event type name).
        /// <para/>
        /// Each Map event type may have any number of supertypes, each supertype must also be of a Map-type event. </summary>
        /// <param name="mapeventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="mapSupertypeName">the name of a Map event type that is the supertype</param>
        public void AddMapSuperType(String mapeventTypeName, String mapSupertypeName)
        {
            var current = _mapTypeConfigurations.Get(mapeventTypeName);
            if (current == null)
            {
                current = new ConfigurationEventTypeMap();
                _mapTypeConfigurations.Put(mapeventTypeName, current);
            }
            var superTypes = current.SuperTypes;
            superTypes.Add(mapSupertypeName);
        }

        /// <summary>
        /// Add, for a given Object-array event type identified by the first parameter, the 
        /// supertype (by its event type name). 
        /// <para/>
        /// Each Object array event type may have any number of supertypes, each supertype must 
        /// also be of a Object-array-type event.
        /// </summary>
        /// <param name="eventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="supertypeName">the name of a Map event type that is the supertype</param>
        public void AddObjectArraySuperType(String eventTypeName, String supertypeName)
        {
            var current = _objectArrayTypeConfigurations.Get(eventTypeName);
            if (current == null)
            {
                current = new ConfigurationEventTypeObjectArray();
                _objectArrayTypeConfigurations.Put(eventTypeName, current);
            }

            var superTypes = current.SuperTypes;
            if (!superTypes.IsEmpty())
            {
                throw new ConfigurationException("Object-array event types may not have multiple supertypes");
            }

            superTypes.Add(supertypeName);
        }

        /// <summary>
        /// Add configuration for a map event type.
        /// <param name="mapeventTypeName">configuration to add</param>
        /// <param name="config">config map type configuration</param>
        /// </summary>
        public void AddMapConfiguration(String mapeventTypeName, ConfigurationEventTypeMap config)
        {
            _mapTypeConfigurations.Put(mapeventTypeName, config);
        }

        /// <summary>Add configuration for a object array event type. </summary>
        /// <param name="objectArrayeventTypeName">configuration to add</param>
        /// <param name="config">map type configuration</param>
        public void AddObjectArrayConfiguration(String objectArrayeventTypeName, ConfigurationEventTypeObjectArray config)
        {
            _objectArrayTypeConfigurations.Put(objectArrayeventTypeName, config);
        }

        /// <summary>Add an name for an event type that represents System.Xml.XmlNode events. </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="xmlDOMEventTypeDesc">descriptor containing property and mapping information for XML-DOM events</param>
        public void AddEventType(String eventTypeName, ConfigurationEventTypeXMLDOM xmlDOMEventTypeDesc)
        {
            _eventTypesXmldom.Put(eventTypeName, xmlDOMEventTypeDesc);
        }

        /// <summary>
        /// Add an name for an event type that represents legacy type events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        public void AddEventType<T>(ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(typeof(T).FullName, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        /// <summary>
        /// Add an name for an event type that represents legacy type events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        public void AddEventType<T>(String eventTypeName, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            AddEventType(eventTypeName, typeof(T).AssemblyQualifiedName, legacyEventTypeDesc);
        }

        /// <summary>Add an name for an event type that represents legacy type events. </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventType">fully-qualified class name of the event type</param>
        /// <param name="legacyEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        public void AddEventType(String eventTypeName, String eventType, ConfigurationEventTypeLegacy legacyEventTypeDesc)
        {
            _eventClasses.Put(eventTypeName, eventType);
            _eventTypesLegacy.Put(eventTypeName, legacyEventTypeDesc);
        }

        public void AddEventType(String eventTypeName, String[] propertyNames, Object[] propertyTypes) 
        {
            LinkedHashMap<String, Object> propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
            _nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
        }

        public void AddEventType(String eventTypeName, String[] propertyNames, Object[] propertyTypes, ConfigurationEventTypeObjectArray config) 
        {
            LinkedHashMap<String, Object> propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
            _nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
            _objectArrayTypeConfigurations.Put(eventTypeName, config);
            if (config.SuperTypes != null && config.SuperTypes.Count > 1)
            {
                throw new ConfigurationException(ConfigurationEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
            }
        }

        public void AddRevisionEventType(String revisionEventTypeName, ConfigurationRevisionEventType revisionEventTypeConfig)
        {
            _revisionEventTypes.Put(revisionEventTypeName, revisionEventTypeConfig);
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
                AddImport(importParts[0], importParts[1]);
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
        /// <param name="autoImport">The auto import.</param>
        public void AddImport(Type autoImport)
        {
            AddImport(autoImport.AssemblyQualifiedName);
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
        /// Removes the import.
        /// </summary>
        /// <param name="name">The name.</param>
        public void RemoveImport(String name)
        {
            var desc = _imports.FirstOrDefault(import => import.TypeOrNamespace == name);
            if (desc != null)
            {
                _imports.Remove(desc);
            }
        }

        /// <summary>
        /// Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="className">the class name (simple or fully-qualified) providing methods</param>
        /// <param name="methodInvocationConfig">the cache configuration</param>
        public void AddMethodRef(String className, ConfigurationMethodRef methodInvocationConfig)
        {
            _methodInvocationReferences[className] = methodInvocationConfig;
        }

        /// <summary>
        /// Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="clazz">the class providing methods</param>
        /// <param name="methodInvocationConfig">the cache configuration</param>
        public void AddMethodRef(Type clazz, ConfigurationMethodRef methodInvocationConfig)
        {
            _methodInvocationReferences[clazz.FullName] = methodInvocationConfig;
        }

        /// <summary>
        /// Returns the mapping of event type name to type name.
        /// </summary>
        public IDictionary<String, String> EventTypeNames
        {
            get { return _eventClasses; }
        }

        /// <summary>
        /// Returns a map keyed by event type name name, and values being the definition for the
        /// event type of the property names and types that make up the event.
        /// </summary>
        /// <returns> map of event type name name and definition of event properties
        /// </returns>
        public IDictionary<String, Properties> EventTypesMapEvents
        {
            get { return _mapNames; }
        }

        public IDictionary<String, IDictionary<String, Object>> EventTypesNestableMapEvents
        {
            get { return _nestableMapNames; }
        }

        public IDictionary<string, DataMap> EventTypesNestableObjectArrayEvents
        {
            get { return _nestableObjectArrayNames; }
        }

        /// <summary> Returns the mapping of event type name to XML DOM event type information.</summary>
        /// <returns> event type aliases mapping to XML DOM configs
        /// </returns>
        public IDictionary<String, ConfigurationEventTypeXMLDOM> EventTypesXMLDOM
        {
            get { return _eventTypesXmldom; }
        }

        /// <summary> Returns the mapping of event type name to legacy event type information.</summary>
        /// <returns> event type aliases mapping to legacy type configs
        /// </returns>
        public IDictionary<String, ConfigurationEventTypeLegacy> EventTypesLegacy
        {
            get { return _eventTypesLegacy; }
        }

        /// <summary> Returns the class and package imports.</summary>
        /// <returns> imported names
        /// </returns>
        public IList<AutoImportDesc> Imports
        {
            get { return _imports; }
        }

        /// <summary> Returns a map of string database names to database configuration options.</summary>
        /// <returns> map of database configurations
        /// </returns>
        public IDictionary<String, ConfigurationDBRef> DatabaseReferences
        {
            get { return _databaseReferences; }
        }

        /// <summary>Returns a list of configured plug-in views.</summary>
        /// <returns>list of plug-in view configs</returns>

        public IList<ConfigurationPlugInView> PlugInViews
        {
            get { return _plugInViews; }
        }

        public IDictionary<string, ConfigurationEventTypeObjectArray> ObjectArrayTypeConfigurations
        {
            get { return _objectArrayTypeConfigurations; }
        }

        /// <summary>
        /// Returns the plug in virtual data windows.
        /// </summary>
        /// <value>The plug in virtual data windows.</value>
        public IList<ConfigurationPlugInVirtualDataWindow> PlugInVirtualDataWindows
        {
            get { return _plugInVirtualDataWindows; }
        }

        /// <summary>Returns a list of configured plugin loaders.</summary>
        /// <returns>plugin loaders</returns>

        public IList<ConfigurationPluginLoader> PluginLoaders
        {
            get { return _pluginLoaders; }
        }

        /// <summary>Returns a list of configured plug-in aggregation functions.</summary>
        /// <returns>list of configured aggregations</returns>

        public IList<ConfigurationPlugInAggregationFunction> PlugInAggregationFunctions
        {
            get { return _plugInAggregationFunctions; }
        }

        public IList<ConfigurationPlugInAggregationMultiFunction> PlugInAggregationMultiFunctions
        {
            get { return _plugInAggregationMultiFunctions; }
        }

        public IList<ConfigurationPlugInSingleRowFunction> PlugInSingleRowFunctions
        {
            get { return _plugInSingleRowFunctions; }
        }

        /// <summary>Returns a list of configured plug-ins for pattern observers and guards.</summary>
        /// <returns>list of pattern plug-ins</returns>

        public IList<ConfigurationPlugInPatternObject> PlugInPatternObjects
        {
            get { return _plugInPatternObjects; }
        }

        /// <summary>
        /// Gets the variables.
        /// </summary>
        /// <value>The variables.</value>
        public IDictionary<String, ConfigurationVariable> Variables
        {
            get { return _variables; }
        }

        /// <summary>
        /// Gets the method invocation references.
        /// </summary>
        /// <value>The method invocation references.</value>
        public IDictionary<String, ConfigurationMethodRef> MethodInvocationReferences
        {
            get { return _methodInvocationReferences; }
        }

        public IDictionary<String, ConfigurationRevisionEventType> RevisionEventTypes
        {
            get { return _revisionEventTypes; }
        }

        public IDictionary<String, ConfigurationEventTypeMap> MapTypeConfigurations
        {
            get { return _mapTypeConfigurations; }
        }

        /// <summary>Add an input/output plugin loader.</summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="typeName">is the fully-qualified classname of the loader class</param>
        /// <param name="configuration">is loader cofiguration entries</param>

        public void AddPluginLoader(String loaderName, String typeName, Properties configuration)
        {
            AddPluginLoader(loaderName, typeName, configuration, null);
        }

        /// <summary>
        /// Add a plugin loader (f.e. an input/output adapter loader) without any additional loader configuration.
        /// <p>
        /// The class is expected to implement <seealso cref="com.espertech.esper.plugin.PluginLoader" />
        /// </p>.
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="className">is the fully-qualified classname of the loader class</param>
        public void AddPluginLoader(String loaderName, String className)
        {
            AddPluginLoader(loaderName, className, null, null);
        }

        /// <summary>
        /// Add a plugin loader (f.e. an input/output adapter loader).
        /// <p>
        /// The class is expected to implement <seealso cref="com.espertech.esper.plugin.PluginLoader" />
        /// </p>.
        /// </summary>
        /// <param name="loaderName">is the name of the loader</param>
        /// <param name="typeName">is the fully-qualified classname of the loader class</param>
        /// <param name="configuration">is loader cofiguration entries</param>
        /// <param name="configurationXML">config xml if any</param>
        public void AddPluginLoader(String loaderName, String typeName, Properties configuration, String configurationXML)
        {
            var pluginLoader = new ConfigurationPluginLoader();
            pluginLoader.LoaderName = loaderName;
            pluginLoader.TypeName = typeName;
            pluginLoader.ConfigProperties = configuration;
            pluginLoader.ConfigurationXML = configurationXML;
            _pluginLoaders.Add(pluginLoader);
        }

        /// <summary>Add a view for plug-in.</summary>
        /// <param name="namespace">is the namespace the view should be available under</param>
        /// <param name="name">is the name of the view</param>
        /// <param name="viewFactoryClass">is the view factory class to use</param>

        public void AddPlugInView(String @namespace, String name, String viewFactoryClass)
        {
            var configurationPlugInView = new ConfigurationPlugInView
            {
                Namespace = @namespace,
                Name = name,
                FactoryClassName = viewFactoryClass
            };
            _plugInViews.Add(configurationPlugInView);
        }

        /// <summary>Add a virtual data window for plug-in. </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="factoryClass">is the view factory class to use</param>
        public void AddPlugInVirtualDataWindow(String @namespace, String name, String factoryClass)
        {
            AddPlugInVirtualDataWindow(@namespace, name, factoryClass, null);
        }

        /// <summary>Add a virtual data window for plug-in. </summary>
        /// <param name="namespace">is the namespace the virtual data window should be available under</param>
        /// <param name="name">is the name of the data window</param>
        /// <param name="factoryClass">is the view factory class to use</param>
        /// <param name="customConfigurationObject">additional configuration to be passed along</param>
        public void AddPlugInVirtualDataWindow(String @namespace, String name, String factoryClass, Object customConfigurationObject)
        {
            ConfigurationPlugInVirtualDataWindow configurationPlugInVirtualDataWindow = new ConfigurationPlugInVirtualDataWindow();
            configurationPlugInVirtualDataWindow.Namespace = @namespace;
            configurationPlugInVirtualDataWindow.Name = name;
            configurationPlugInVirtualDataWindow.FactoryClassName = factoryClass;
            configurationPlugInVirtualDataWindow.Config = customConfigurationObject;
            _plugInVirtualDataWindows.Add(configurationPlugInVirtualDataWindow);
        }

        /// <summary>Add a pattern event observer for plug-in.</summary>
        /// <param name="namespace">is the namespace the observer should be available under</param>
        /// <param name="name">is the name of the observer</param>
        /// <param name="observerFactoryClass">is the observer factory class to use</param>

        public void AddPlugInPatternObserver(String @namespace, String name, String observerFactoryClass)
        {
            var entry = new ConfigurationPlugInPatternObject
            {
                Namespace = @namespace,
                Name = name,
                FactoryClassName = observerFactoryClass,
                PatternObjectType = ConfigurationPlugInPatternObject.PatternObjectTypeEnum.OBSERVER
            };
            _plugInPatternObjects.Add(entry);
        }

        /// <summary>Add a pattern guard for plug-in.</summary>
        /// <param name="namespace">is the namespace the guard should be available under</param>
        /// <param name="name">is the name of the guard</param>
        /// <param name="guardFactoryClass">is the guard factory class to use</param>

        public void AddPlugInPatternGuard(String @namespace, String name, String guardFactoryClass)
        {
            var entry = new ConfigurationPlugInPatternObject
            {
                Namespace = @namespace,
                Name = name,
                FactoryClassName = guardFactoryClass,
                PatternObjectType = ConfigurationPlugInPatternObject.PatternObjectTypeEnum.GUARD
            };
            _plugInPatternObjects.Add(entry);
        }

        public void AddEventTypeAutoName(String @namespace)
        {
            _eventTypeAutoNamePackages.Add(@namespace);
        }

        public void AddVariable<T>(string variableName, T initializationValue)
        {
            AddVariable(variableName, initializationValue, false);
        }

        /// <summary>
        /// Add variable that can be a constant.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="initializationValue">The initialization value.</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        public void AddVariable<T>(String variableName, T initializationValue, Boolean constant = false)
        {
            ConfigurationVariable configVar = new ConfigurationVariable();
            configVar.VariableType = typeof(T).GetBoxedType().FullName;
            configVar.InitializationValue = initializationValue;
            configVar.IsConstant = constant;
            _variables[variableName] = configVar;
        }

        public void AddVariable(string variableName, Type type, object initializationValue)
        {
            AddVariable(variableName, type, initializationValue, false);
        }

        public void AddVariable(String variableName, Type type, Object initializationValue, Boolean constant = false)
        {
            ConfigurationVariable configVar = new ConfigurationVariable();
            configVar.VariableType = type.FullName;
            configVar.InitializationValue = initializationValue;
            configVar.IsConstant = constant;
            _variables[variableName] = configVar;
        }

        public void AddVariable(String variableName, String type, Object initializationValue, Boolean constant = false)
        {
            ConfigurationVariable configVar = new ConfigurationVariable();
            configVar.VariableType = type;
            configVar.InitializationValue = initializationValue;
            configVar.IsConstant = constant;
            _variables[variableName] = configVar;
        }

        /// <summary>Adds an event representation responsible for creating event types (event metadata) and event bean instances (events) fora certain kind of object representation that holds the event property values.</summary>
        /// <param name="eventRepresentationRootURI">uniquely identifies the event representation and acts as a parentfor child URIs used in resolving</param>
        /// <param name="eventRepresentationClassName">is the name of the class implementing <see cref="com.espertech.esper.plugin.PlugInEventRepresentation"/>.</param>
        /// <param name="initializer">is optional configuration or initialization information, or null if none required</param>
        public void AddPlugInEventRepresentation(Uri eventRepresentationRootURI, String eventRepresentationClassName, Object initializer)
        {
            var config = new ConfigurationPlugInEventRepresentation
            {
                EventRepresentationTypeName =
                    eventRepresentationClassName,
                Initializer = initializer
            };
            _plugInEventRepresentation.Put(eventRepresentationRootURI, config);
        }

        /// <summary>Adds an event representation responsible for creating event types (event metadata) and event bean instances (events) fora certain kind of object representation that holds the event property values.</summary>
        /// <param name="eventRepresentationRootURI">uniquely identifies the event representation and acts as a parentfor child URIs used in resolving</param>
        /// <param name="eventRepresentationType">is the class implementing <see cref="com.espertech.esper.plugin.PlugInEventRepresentation"/>.</param>
        /// <param name="initializer">is optional configuration or initialization information, or null if none required</param>
        public void AddPlugInEventRepresentation(Uri eventRepresentationRootURI, Type eventRepresentationType, Object initializer)
        {
            AddPlugInEventRepresentation(eventRepresentationRootURI, eventRepresentationType.FullName, initializer);
        }

        public void AddPlugInEventType(String eventTypeName, IList<Uri> resolutionURIs, Object initializer)
        {
            var config = new ConfigurationPlugInEventType
            {
                EventRepresentationResolutionURIs = resolutionURIs,
                Initializer = initializer
            };
            _plugInEventTypes.Put(eventTypeName, config);
        }

        public IList<Uri> PlugInEventTypeResolutionURIs { get; set; }

        public IDictionary<Uri, ConfigurationPlugInEventRepresentation> PlugInEventRepresentation
        {
            get { return _plugInEventRepresentation; }
        }

        public IDictionary<String, ConfigurationPlugInEventType> PlugInEventTypes
        {
            get { return _plugInEventTypes; }
        }

        /// <summary>
        /// Returns a set of namespaces that event classes reside in.
        /// <para>
        /// This setting allows an application to place all it's events into one or more namespaces
        /// and then declare these packages via this method. The engine
        /// attempts to resolve an event type name to a type residing in each declared package.
        /// </para>
        /// <para>
        /// For example, in the statement "select * from MyEvent" the engine attempts to load
        /// class "namespace.MyEvent" and if successful, uses that class as the event type.
        /// </para>
        /// </summary>
        public ICollection<String> EventTypeAutoNamePackages
        {
            get { return _eventTypeAutoNamePackages; }
        }

        /// <summary>
        /// Gets the engine default settings.
        /// </summary>
        /// <value>The engine defaults.</value>
        public ConfigurationEngineDefaults EngineDefaults
        {
            get { return _engineDefaults; }
        }

        public void AddVariantStream(String variantEventTypeName, ConfigurationVariantStream variantStreamConfig)
        {
            _variantStreams.Put(variantEventTypeName, variantStreamConfig);
        }

        /// <summary>
        /// Gets the variant streams.
        /// </summary>
        /// <value>The variant streams.</value>
        public IDictionary<string, ConfigurationVariantStream> VariantStreams
        {
            get { return _variantStreams; }
        }

        public void UpdateMapEventType(String mapEventTypeName, IDictionary<String, Object> typeMap)
        {
            throw new UnsupportedOperationException("Map type Update is only available in runtime configuration");
        }

        public void UpdateObjectArrayEventType(String myEvent, String[] namesNew, Object[] typesNew)
        {
            throw new UnsupportedOperationException("Object-array type Update is only available in runtime configuration");
        }

        public void ReplaceXMLEventType(String xmlEventTypeName, ConfigurationEventTypeXMLDOM config)
        {
            throw new UnsupportedOperationException("XML type Update is only available in runtime configuration");
        }

        public ICollection<String> GetEventTypeNameUsedBy(String name)
        {
            throw new UnsupportedOperationException("Get event type by name is only available in runtime configuration");
        }

        public bool IsVariantStreamExists(String name)
        {
            return _variantStreams.ContainsKey(name);
        }

        public void SetMetricsReportingInterval(String stmtGroupName, long newInterval)
        {
            EngineDefaults.MetricsReportingConfig.SetStatementGroupInterval(stmtGroupName, newInterval);
        }

        public void SetMetricsReportingStmtEnabled(String statementName)
        {
            throw new UnsupportedOperationException("Statement metric reporting can only be enabled or disabled at runtime");
        }

        public void SetMetricsReportingStmtDisabled(String statementName)
        {
            throw new UnsupportedOperationException("Statement metric reporting can only be enabled or disabled at runtime");
        }

        public EventType GetEventType(String eventTypeName)
        {
            throw new UnsupportedOperationException("Obtaining an event type by name is only available at runtime");
        }

        public ICollection<EventType> EventTypes
        {
            get { throw new UnsupportedOperationException("Obtaining event types is only available at runtime"); }
        }

        public void SetMetricsReportingEnabled()
        {
            EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting = true;
        }

        public void SetMetricsReportingDisabled()
        {
            EngineDefaults.MetricsReportingConfig.IsEnableMetricsReporting = false;
        }

        public long PatternMaxSubexpressions
        {
            get { return EngineDefaults.PatternsConfig.MaxSubexpressions.GetValueOrDefault(); }
            set { EngineDefaults.PatternsConfig.MaxSubexpressions = value; }
        }

        public bool RemoveEventType(String eventTypeName, bool force)
        {
            _eventClasses.Remove(eventTypeName);
            _eventTypesXmldom.Remove(eventTypeName);
            _eventTypesLegacy.Remove(eventTypeName);
            _mapNames.Remove(eventTypeName);
            _nestableMapNames.Remove(eventTypeName);
            _mapTypeConfigurations.Remove(eventTypeName);
            _plugInEventTypes.Remove(eventTypeName);
            _revisionEventTypes.Remove(eventTypeName);
            _variantStreams.Remove(eventTypeName);
            return true;
        }

        public ICollection<String> GetVariableNameUsedBy(String variableName)
        {
            throw new UnsupportedOperationException(
                "Get variable use information is only available in runtime configuration");
        }

        public bool RemoveVariable(String name, bool force)
        {
            return _variables.Remove(name);
        }

        /// <summary> Use the configuration specified in an application
        /// resource named <tt>esper.cfg.xml</tt>.
        /// </summary>
        /// <returns> Configuration initialized from the resource
        /// </returns>
        /// <throws>  EPException thrown to indicate error reading configuration </throws>
        public virtual Configuration Configure()
        {
            Configure("/" + ESPER_DEFAULT_CONFIG);
            return this;
        }

        /// <summary> Use the configuration specified in the given application
        /// resource. The format of the resource is defined in
        /// <tt>esper-configuration-2.0.xsd</tt>.
        /// <p/>
        /// The resource is found via <tt>getConfigurationInputStream(resource)</tt>.
        /// That method can be overridden to implement an arbitrary lookup strategy.
        /// <p/>
        /// See <tt>getResourceAsStream</tt> for information on how the resource name is resolved.
        /// </summary>
        /// <param name="resource">if the file name of the resource
        /// </param>
        /// <returns> Configuration initialized from the resource
        /// </returns>
        /// <throws>  EPException thrown to indicate error reading configuration </throws>
        public virtual Configuration Configure(String resource)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("Configuring from resource: " + resource);
            }
            Stream stream = GetConfigurationInputStream(resource);
            ConfigurationParser.DoConfigure(this, stream, resource);
            return this;
        }

        /// <summary> Get the configuration file as an <tt>InputStream</tt>. Might be overridden
        /// by subclasses to allow the configuration to be located by some arbitrary
        /// mechanism.
        ///
        /// See GetResourceAsStream for information on how the resource name is resolved.
        /// </summary>
        /// <param name="resource">is the resource name
        /// </param>
        /// <returns> input stream for resource
        /// </returns>
        /// <throws>  EPException thrown to indicate error reading configuration </throws>
        internal static Stream GetConfigurationInputStream(String resource)
        {
            return GetResourceAsStream(resource);
        }


        /// <summary> Use the configuration specified by the given URL.
        /// The format of the document obtained from the URL is defined in
        /// <tt>esper-configuration-2.0.xsd</tt>.
        ///
        /// </summary>
        /// <param name="url">URL from which you wish to load the configuration
        /// </param>
        /// <returns> A configuration configured via the file
        /// </returns>
        /// <throws>  EPException </throws>
        public virtual Configuration Configure(Uri url)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("configuring from url: " + url);
            }
            try
            {
                ConfigurationParser.DoConfigure(this, WebRequest.Create(url).GetResponse().GetResponseStream(), url.ToString());
                return this;
            }
            catch (IOException ioe)
            {
                throw new EPException("could not configure from URL: " + url, ioe);
            }
        }

        /// <summary> Use the configuration specified in the given application
        /// file. The format of the file is defined in
        /// <tt>esper-configuration-2.0.xsd</tt>.
        ///
        /// </summary>
        /// <param name="configFile"><tt>File</tt> from which you wish to load the configuration
        /// </param>
        /// <returns> A configuration configured via the file
        /// </returns>
        /// <throws>  EPException </throws>
        public virtual Configuration Configure(FileInfo configFile)
        {
            if (Log.IsDebugEnabled)
            {
                Log.Debug("configuring from file: " + configFile.Name);
            }
            try
            {
                using (var stream = new FileStream(configFile.FullName, FileMode.Open, FileAccess.Read))
                {
                    ConfigurationParser.DoConfigure(this, stream, configFile.ToString());
                }
            }
            catch (FileNotFoundException fnfe)
            {
                throw new EPException("could not find file: " + configFile, fnfe);
            }
            return this;
        }

        /// <summary> Use the mappings and properties specified in the given XML document.
        /// The format of the file is defined in
        /// <tt>esper-configuration-2.0.xsd</tt>.
        ///
        /// </summary>
        /// <param name="document">an XML document from which you wish to load the configuration
        /// </param>
        /// <returns> A configuration configured via the <tt>Document</tt>
        /// </returns>
        /// <throws>  EPException if there is problem in accessing the document. </throws>
        public virtual Configuration Configure(XmlDocument document)
        {
            Log.Debug("configuring from XML document");
            ConfigurationParser.DoConfigure(this, document);
            return this;
        }

        /// <summary> Returns an input stream from an application resource in the classpath.
        ///
        /// The method first removes the '/' character from the resource name if
        /// the first character is '/'.
        ///
        /// The lookup order is as follows:
        ///
        /// If a thread context class loader exists, use <tt>Thread.CurrentThread().getResourceAsStream</tt>
        /// to obtain an InputStream.
        ///
        /// If no input stream was returned, use the <tt>typeof(Configuration).getResourceAsStream</tt>.
        /// to obtain an InputStream.
        ///
        /// If no input stream was returned, use the <tt>typeof(Configuration).GetClassLoader().getResourceAsStream</tt>.
        /// to obtain an InputStream.
        ///
        /// If no input stream was returned, throw an Exception.
        ///
        /// </summary>
        /// <param name="resource">to get input stream for
        /// </param>
        /// <returns> input stream for resource
        /// </returns>
        internal static Stream GetResourceAsStream(String resource)
        {
            String stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;
            Stream stream = ResourceManager.GetResourceAsStream(resource) ??
                            ResourceManager.GetResourceAsStream(stripped);
            if (stream == null)
            {
                throw new EPException(resource + " not found");
            }
            return stream;
        }

        /// <summary> Reset to an empty configuration.</summary>
        internal void Reset()
        {
            _eventClasses = new Dictionary<String, String>();
            _mapNames = new Dictionary<String, Properties>();
            _nestableMapNames = new Dictionary<String, IDictionary<String, Object>>();
            _nestableObjectArrayNames = new Dictionary<String, IDictionary<String, Object>>();
            _eventTypesXmldom = new Dictionary<String, ConfigurationEventTypeXMLDOM>();
            _eventTypesLegacy = new Dictionary<String, ConfigurationEventTypeLegacy>();
            _databaseReferences = new Dictionary<String, ConfigurationDBRef>();
            _imports = new List<AutoImportDesc>();
            AddDefaultImports();
            _plugInViews = new List<ConfigurationPlugInView>();
            _plugInVirtualDataWindows = new List<ConfigurationPlugInVirtualDataWindow>();
            _pluginLoaders = new List<ConfigurationPluginLoader>();
            _plugInAggregationFunctions = new List<ConfigurationPlugInAggregationFunction>();
            _plugInAggregationMultiFunctions = new List<ConfigurationPlugInAggregationMultiFunction>();
            _plugInSingleRowFunctions = new List<ConfigurationPlugInSingleRowFunction>();
            _plugInPatternObjects = new List<ConfigurationPlugInPatternObject>();
            _engineDefaults = new ConfigurationEngineDefaults();
            _eventTypeAutoNamePackages = new FIFOHashSet<String>();
            _variables = new Dictionary<String, ConfigurationVariable>();
            _methodInvocationReferences = new Dictionary<String, ConfigurationMethodRef>();
            _plugInEventRepresentation = new Dictionary<Uri, ConfigurationPlugInEventRepresentation>();
            _plugInEventTypes = new Dictionary<String, ConfigurationPlugInEventType>();
            _revisionEventTypes = new Dictionary<String, ConfigurationRevisionEventType>();
            _variantStreams = new Dictionary<String, ConfigurationVariantStream>();
            _mapTypeConfigurations = new Dictionary<String, ConfigurationEventTypeMap>();
            _objectArrayTypeConfigurations = new HashMap<String, ConfigurationEventTypeObjectArray>();
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
            _imports.Add(new AutoImportDesc(typeof (BeaconSource).Namespace, null));
        }
    }

    /// <summary>
    /// Enumeration of event representation
    /// </summary>
    public enum EventRepresentation
    {
        /// <summary>
        /// Event representation is object-array (Object[]).
        /// </summary>
        OBJECTARRAY,

        /// <summary>
        /// Event representation is Map (any IDictionary interface implementation).
        /// </summary>

        MAP
    }

    public static class EventRepresentationExtensions
    {
        public static EventRepresentation Default
        {
            get { return EventRepresentation.MAP; }
        }
    }
}
