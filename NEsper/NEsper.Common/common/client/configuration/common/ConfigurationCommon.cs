///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.epl.dataflow.ops;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Contains settings that apply to both the compile-time and the runtime.
    /// </summary>
    [Serializable]
    public class ConfigurationCommon
    {
        /// <summary>
        ///     Import name of the package that hosts the annotation classes.
        /// </summary>
        public static readonly string ANNOTATION_IMPORT = typeof(NameAttribute).Namespace + ".*";

        /// <summary>
        ///     Import package for data flow operator forges.
        /// </summary>
        public static readonly string DATAFLOWOPERATOR_IMPORT = typeof(BeaconSourceForge).Namespace + ".*";

        /// <summary>
        ///     For annotations only, the class and package name imports that
        ///     will be used to resolve partial class names (not available in EPL statements unless used in an annotation).
        /// </summary>
        protected IList<string> annotationImports;

        /// <summary>
        ///     The class and package name imports that
        ///     will be used to resolve partial class names.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonDBRef> databaseReferences;

        /// <summary>
        ///     Map of event name and fully-qualified class name.
        /// </summary>
        protected IDictionary<string, string> eventClasses;

        /// <summary>
        ///     Event type common configuration
        /// </summary>
        protected ConfigurationCommonEventTypeMeta eventMeta;

        /// <summary>
        ///     Event type auto-name packages.
        /// </summary>
        private ISet<string> eventTypeAutoNamePackages;

        /// <summary>
        ///     Map of event type name and XML DOM configuration.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonEventTypeAvro> eventTypesAvro;

        /// <summary>
        ///     Map of event type name and bean-type event configuration.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonEventTypeBean> eventTypesBean;

        /// <summary>
        ///     Map of event type name and XML DOM configuration.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonEventTypeXMLDOM> eventTypesXMLDOM;

        /// <summary>
        ///     Execution-related configuration
        /// </summary>
        protected ConfigurationCommonExecution execution;

        /// <summary>
        ///     The class and package name imports that
        ///     will be used to resolve partial class names.
        /// </summary>
        protected IList<string> imports;

        /// <summary>
        ///     Logging configuration.
        /// </summary>
        protected ConfigurationCommonLogging logging;

        /// <summary>
        ///     The type names for events that are backed by java.util.Map,
        ///     not containing strongly-typed nested maps.
        /// </summary>
        protected IDictionary<string, Properties> mapNames;

        /// <summary>
        ///     Map event types additional configuration information.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonEventTypeMap> mapTypeConfigurations;

        /// <summary>
        ///     Map of class name and configuration for method invocations on that class.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonMethodRef> methodInvocationReferences;

        /// <summary>
        ///     The type names for events that are backed by java.util.Map,
        ///     possibly containing strongly-typed nested maps.
        ///     <para />
        ///     Each entrie's value must be either a Class or a Map&lt;String,Object&gt; to
        ///     define nested maps.
        /// </summary>
        protected IDictionary<string, IDictionary<string, object>> nestableMapNames;

        /// <summary>
        ///     The type names for events that are backed by java.util.Map,
        ///     possibly containing strongly-typed nested maps.
        ///     <para />
        ///     Each entrie's value must be either a Class or a Map&lt;String,Object&gt; to
        ///     define nested maps.
        /// </summary>
        protected IDictionary<string, IDictionary<string, object>> nestableObjectArrayNames;

        /// <summary>
        ///     Map event types additional configuration information.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonEventTypeObjectArray> objectArrayTypeConfigurations;

        /// <summary>
        ///     Time source configuration
        /// </summary>
        protected ConfigurationCommonTimeSource timeSource;

        /// <summary>
        ///     Transient configuration.
        /// </summary>
        [NonSerialized] protected IDictionary<string, object> transientConfiguration;

        /// <summary>
        ///     Map of variables.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonVariable> variables;

        /// <summary>
        ///     Variant streams allow events of disparate types to be treated the same.
        /// </summary>
        protected IDictionary<string, ConfigurationCommonVariantStream> variantStreams;

        /// <summary>
        ///     Constructs an empty configuration. The auto import values
        ///     are set by default to System and System.Text.
        /// </summary>
        public ConfigurationCommon()
        {
            Reset();
        }

        /// <summary>
        ///     Returns the mapping of event type name to type name.
        /// </summary>
        /// <value>event type names for type names</value>
        public IDictionary<string, string> EventTypeNames => eventClasses;

        /// <summary>
        ///     Returns a map keyed by event type name, and values being the definition for the
        ///     Map event type of the property names and types that make up the event.
        /// </summary>
        /// <value>map of event type name and definition of event properties</value>
        public IDictionary<string, Properties> EventTypesMapEvents => mapNames;

        /// <summary>
        ///     Returns a map keyed by event type name, and values being the definition for the
        ///     event type of the property names and types that make up the event,
        ///     for nestable, strongly-typed Map-based event representations.
        /// </summary>
        /// <value>map of event type name and definition of event properties</value>
        public IDictionary<string, IDictionary<string, object>> EventTypesNestableMapEvents => nestableMapNames;

        /// <summary>
        ///     Returns the object-array event types.
        /// </summary>
        /// <value>object-array event types</value>
        public IDictionary<string, IDictionary<string, object>> EventTypesNestableObjectArrayEvents => nestableObjectArrayNames;

        /// <summary>
        ///     Returns the mapping of event type name to XML DOM event type information.
        /// </summary>
        /// <value>event type name mapping to XML DOM configs</value>
        public IDictionary<string, ConfigurationCommonEventTypeXMLDOM> EventTypesXMLDOM => eventTypesXMLDOM;

        /// <summary>
        ///     Returns the Avro event types.
        /// </summary>
        /// <value>Avro event types</value>
        public IDictionary<string, ConfigurationCommonEventTypeAvro> EventTypesAvro => eventTypesAvro;

        /// <summary>
        ///     Returns the mapping of event type name to legacy java event type information.
        /// </summary>
        /// <value>event type name mapping to legacy java class configs</value>
        public IDictionary<string, ConfigurationCommonEventTypeBean> EventTypesBean => eventTypesBean;

        /// <summary>
        ///     Returns the imports
        /// </summary>
        /// <value>imports</value>
        public IList<string> Imports => imports;

        /// <summary>
        ///     Returns the annotation imports
        /// </summary>
        /// <value>annotation imports</value>
        public IList<string> AnnotationImports => annotationImports;

        /// <summary>
        ///     Returns the database names
        /// </summary>
        /// <value>database names</value>
        public IDictionary<string, ConfigurationCommonDBRef> DatabaseReferences => databaseReferences;

        /// <summary>
        ///     Returns the object-array event type configurations.
        /// </summary>
        /// <value>type configs</value>
        public IDictionary<string, ConfigurationCommonEventTypeObjectArray> ObjectArrayTypeConfigurations => objectArrayTypeConfigurations;

        /// <summary>
        ///     Returns the preconfigured variables
        /// </summary>
        /// <value>variables</value>
        public IDictionary<string, ConfigurationCommonVariable> Variables => variables;

        /// <summary>
        ///     Returns the method-invocation-names for use in joins
        /// </summary>
        /// <value>method-invocation-names</value>
        public IDictionary<string, ConfigurationCommonMethodRef> MethodInvocationReferences => methodInvocationReferences;

        /// <summary>
        ///     Returns for each Map event type name the set of supertype event type names (Map types only).
        /// </summary>
        /// <value>map of name to set of supertype names</value>
        public IDictionary<string, ConfigurationCommonEventTypeMap> MapTypeConfigurations => mapTypeConfigurations;

        /// <summary>
        ///     Returns a map of variant stream name and variant configuration information. Variant streams allows handling
        ///     events of all sorts of different event types the same way.
        /// </summary>
        /// <value>map of name and variant stream config</value>
        public IDictionary<string, ConfigurationCommonVariantStream> VariantStreams => variantStreams;

        /// <summary>
        ///     Returns transient configuration, i.e. information that is passed along as a reference and not as a value
        /// </summary>
        /// <value>map of transients</value>
        public IDictionary<string, object> TransientConfiguration {
            get => transientConfiguration;
            set => transientConfiguration = value;
        }

        /// <summary>
        ///     Returns event representation default settings.
        /// </summary>
        /// <value>event representation default settings</value>
        public ConfigurationCommonEventTypeMeta EventMeta => eventMeta;

        /// <summary>
        ///     Returns the time source configuration.
        /// </summary>
        /// <value>time source enum</value>
        public ConfigurationCommonTimeSource TimeSource => timeSource;

        /// <summary>
        ///     Returns logging settings applicable to common.
        /// </summary>
        /// <value>logging settings</value>
        public ConfigurationCommonLogging Logging => logging;

        /// <summary>
        ///     Returns the execution settings.
        /// </summary>
        /// <value>execution settings</value>
        public ConfigurationCommonExecution Execution => execution;

        /// <summary>
        ///     Returns a set of namespaces that event classes reside in.
        ///     <para />
        ///     This setting allows an application to place all it's events into one or more namespaces
        ///     and then declare these packages via this method. The runtime
        ///     attempts to resolve an event type name to a type residing in each declared package.
        ///     <para />
        ///     For example, in the statement "select * from MyEvent" the runtime attempts to load class "namespace.MyEvent"
        ///     and if successful, uses that class as the event type.
        /// </summary>
        /// <value>set namespaces to look for events types when encountering a new event type name</value>
        public ISet<string> EventTypeAutoNamePackages => eventTypeAutoNamePackages;

        /// <summary>
        ///     Checks if an event type has already been registered for that name.
        /// </summary>
        /// <param name="eventTypeName">the name</param>
        /// <returns>true if already registered</returns>
        /// <unknown>@since 2.1</unknown>
        public bool IsEventTypeExists(string eventTypeName)
        {
            return eventClasses.ContainsKey(eventTypeName)
                   || mapNames.ContainsKey(eventTypeName)
                   || nestableMapNames.ContainsKey(eventTypeName)
                   || nestableObjectArrayNames.ContainsKey(eventTypeName)
                   || eventTypesXMLDOM.ContainsKey(eventTypeName)
                   || eventTypesAvro.ContainsKey(eventTypeName);
        }

        /// <summary>
        ///     Add an name for an event type represented by plain-old object events.
        ///     Note that when adding multiple names for the same type the names represent an
        ///     alias to the same event type since event type identity for classes is per class.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClassName">fully-qualified class name of the event type</param>
        public void AddEventType(
            string eventTypeName,
            string eventClassName)
        {
            eventClasses.Put(eventTypeName, eventClassName);
        }

        /// <summary>
        ///     Add an name for an event type represented by plain-old object events.
        ///     Note that when adding multiple names for the same type the names represent an
        ///     alias to the same event type since event type identity for types is per type.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">is the event class for which to add the name</param>
        public void AddEventType(
            string eventTypeName,
            Type eventClass)
        {
            AddEventType(eventTypeName, eventClass.Name);
        }

        /// <summary>
        ///     Add an name for an event type represented by plain-old object events,
        ///     and the name is the simple class name of the class.
        /// </summary>
        /// <param name="eventClass">is the event class for which to add the name</param>
        public void AddEventType(Type eventClass)
        {
            AddEventType(eventClass.Name, eventClass.Name);
        }

        /// <summary>
        ///     Add an name for an event type that represents java.util.Map events.
        ///     <para />
        ///     Each entry in the type map is the property name and the fully-qualified
        ///     type name or primitive type name.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type(fully qualified classname) of its value in Map event
        ///     instances.
        /// </param>
        public void AddEventType(
            string eventTypeName,
            Properties typeMap)
        {
            mapNames.Put(eventTypeName, typeMap);
        }

        /// <summary>
        ///     Add an name for an event type that represents java.util.Map events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para />
        ///     Each entry in the type mapping must contain the String property name as the key value,
        ///     and either a Class, or a further Map&lt;String, Object&gt;, or the name
        ///     of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type(fully qualified classname) of its value in Map event
        ///     instances.
        /// </param>
        public void AddEventType(
            string eventTypeName,
            IDictionary<string, object> typeMap)
        {
            nestableMapNames.Put(eventTypeName, new LinkedHashMap<string, object>(typeMap));
        }

        /// <summary>
        ///     Add a name for an event type that represents java.util.Map events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para />
        ///     Each entry in the type mapping must contain the String property name as the key value,
        ///     and either a Class, or a further Map&lt;String, Object&gt;, or the name
        ///     of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type(fully qualified classname) of its value in Map event
        ///     instances.
        /// </param>
        /// <param name="superTypes">is an array of event type name of further Map types that this</param>
        public void AddEventType(
            string eventTypeName,
            IDictionary<string, object> typeMap,
            string[] superTypes)
        {
            nestableMapNames.Put(eventTypeName, new LinkedHashMap<string, object>(typeMap));
            if (superTypes != null) {
                for (var i = 0; i < superTypes.Length; i++) {
                    AddMapSuperType(eventTypeName, superTypes[i]);
                }
            }
        }

        /// <summary>
        ///     Add a name for an event type that represents java.util.Map events,
        ///     and for which each property may itself be a Map of further properties,
        ///     with unlimited nesting levels.
        ///     <para />
        ///     Each entry in the type mapping must contain the String property name as the key value,
        ///     and either a Class, or a further Map&lt;String, Object&gt;, or the name
        ///     of another previously-register Map event type (append [] for array of Map).
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="typeMap">
        ///     maps the name of each property in the Map event to the type(fully qualified classname) of its value in Map event
        ///     instances.
        /// </param>
        /// <param name="mapConfig">is the Map-event type configuration that may defined super-types, timestamp-property-name etc.</param>
        public void AddEventType(
            string eventTypeName,
            IDictionary<string, object> typeMap,
            ConfigurationCommonEventTypeMap mapConfig)
        {
            nestableMapNames.Put(eventTypeName, new LinkedHashMap<string, object>(typeMap));
            mapTypeConfigurations.Put(eventTypeName, mapConfig);
        }

        /// <summary>
        ///     Add, for a given Map event type identified by the first parameter, the supertype (by its event type name).
        ///     <para />
        ///     Each Map event type may have any number of supertypes, each supertype must also be of a Map-type event.
        /// </summary>
        /// <param name="mapeventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="mapSupertypeName">the name of a Map event type that is the supertype</param>
        public void AddMapSuperType(
            string mapeventTypeName,
            string mapSupertypeName)
        {
            var current = mapTypeConfigurations.Get(mapeventTypeName);
            if (current == null) {
                current = new ConfigurationCommonEventTypeMap();
                mapTypeConfigurations.Put(mapeventTypeName, current);
            }

            var superTypes = current.SuperTypes;
            superTypes.Add(mapSupertypeName);
        }

        /// <summary>
        ///     Add, for a given Object-array event type identified by the first parameter, the supertype (by its event type name).
        ///     <para />
        ///     Each Object array event type may have any number of supertypes, each supertype must also be of a Object-array-type
        ///     event.
        /// </summary>
        /// <param name="eventTypeName">the name of a Map event type, that is to have a supertype</param>
        /// <param name="supertypeName">the name of a Map event type that is the supertype</param>
        public void AddObjectArraySuperType(
            string eventTypeName,
            string supertypeName)
        {
            var current = objectArrayTypeConfigurations.Get(eventTypeName);
            if (current == null) {
                current = new ConfigurationCommonEventTypeObjectArray();
                objectArrayTypeConfigurations.Put(eventTypeName, current);
            }

            var superTypes = current.SuperTypes;
            if (!superTypes.IsEmpty()) {
                throw new ConfigurationException("Object-array event types may not have multiple supertypes");
            }

            superTypes.Add(supertypeName);
        }

        /// <summary>
        ///     Add configuration for a map event type.
        /// </summary>
        /// <param name="mapeventTypeName">configuration to add</param>
        /// <param name="config">map type configuration</param>
        public void AddMapConfiguration(
            string mapeventTypeName,
            ConfigurationCommonEventTypeMap config)
        {
            mapTypeConfigurations.Put(mapeventTypeName, config);
        }

        /// <summary>
        ///     Add configuration for a object array event type.
        /// </summary>
        /// <param name="objectArrayeventTypeName">configuration to add</param>
        /// <param name="config">map type configuration</param>
        public void AddObjectArrayConfiguration(
            string objectArrayeventTypeName,
            ConfigurationCommonEventTypeObjectArray config)
        {
            objectArrayTypeConfigurations.Put(objectArrayeventTypeName, config);
        }

        /// <summary>
        ///     Add an name for an event type that represents XmlNode events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="xmlDOMEventTypeDesc">descriptor containing property and mapping information for XML-DOM events</param>
        public void AddEventType(
            string eventTypeName,
            ConfigurationCommonEventTypeXMLDOM xmlDOMEventTypeDesc)
        {
            eventTypesXMLDOM.Put(eventTypeName, xmlDOMEventTypeDesc);
        }

        /// <summary>
        ///     Add an event type that represents Object-array (Object[]) events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        public void AddEventType(
            string eventTypeName,
            string[] propertyNames,
            object[] propertyTypes)
        {
            var propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
            nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
        }

        /// <summary>
        ///     Add an event type that represents Object-array (Object[]) events.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="propertyNames">name of each property, length must match number of types</param>
        /// <param name="propertyTypes">type of each property, length must match number of names</param>
        /// <param name="optionalConfiguration">object-array type configuration</param>
        /// <throws>ConfigurationException if the name is already in used for a different type</throws>
        public void AddEventType(
            string eventTypeName,
            string[] propertyNames,
            object[] propertyTypes,
            ConfigurationCommonEventTypeObjectArray optionalConfiguration)
        {
            var propertyTypesMap = EventTypeUtility.ValidateObjectArrayDef(propertyNames, propertyTypes);
            nestableObjectArrayNames.Put(eventTypeName, propertyTypesMap);
            objectArrayTypeConfigurations.Put(eventTypeName, optionalConfiguration);
            if (optionalConfiguration.SuperTypes != null && optionalConfiguration.SuperTypes.Count > 1) {
                throw new ConfigurationException(ConfigurationCommonEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
            }
        }

        /// <summary>
        ///     Add a database reference with a given database name.
        /// </summary>
        /// <param name="name">is the database name</param>
        /// <param name="configurationDBRef">descriptor containing database connection and access policy information</param>
        public void AddDatabaseReference(
            string name,
            ConfigurationCommonDBRef configurationDBRef)
        {
            databaseReferences.Put(name, configurationDBRef);
        }

        /// <summary>
        ///     Add an name for an event type that represents object type events.
        ///     Note that when adding multiple names for the same type the names represent an
        ///     alias to the same event type since event type identity for types is per type.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="eventClass">fully-qualified class name of the event type</param>
        /// <param name="beanEventTypeDesc">descriptor containing property and mapping information for legacy type events</param>
        public void AddEventType(
            string eventTypeName,
            string eventClass,
            ConfigurationCommonEventTypeBean beanEventTypeDesc)
        {
            eventClasses.Put(eventTypeName, eventClass);
            eventTypesBean.Put(eventTypeName, beanEventTypeDesc);
        }

        /// <summary>
        ///     Adds a package or class to the list of automatically-imported classes and packages.
        ///     <para />
        ///     To import a single class offering a static method, simply supply the fully-qualified name of the class
        ///     and use the syntax {@code classname.methodname(...)}
        ///     <para />
        ///     To import a whole package and use the {@code classname.methodname(...)} syntax, specify a package
        ///     with wildcard, such as {@code com.mycompany.staticlib.*}.
        /// </summary>
        /// <param name="autoImport">is a fully-qualified class name or a package name with wildcard</param>
        public void AddImport(string autoImport)
        {
            imports.Add(autoImport);
        }

        /// <summary>
        ///     Adds a class to the list of automatically-imported classes.
        ///     <para />
        ///     Use #addImport(String) to import a package.
        /// </summary>
        /// <param name="autoImport">is a class to import</param>
        public void AddImport(Type autoImport)
        {
            AddImport(autoImport.Name);
        }

        /// <summary>
        ///     Adds a package or class to the list of automatically-imported classes and packages for use by annotations only.
        /// </summary>
        /// <param name="importName">import such as package name, class name, or package with ".*".</param>
        /// <throws>ConfigurationException if incorrect package or class names are encountered</throws>
        public void AddAnnotationImport(string importName)
        {
            annotationImports.Add(importName);
        }

        /// <summary>
        ///     Add a class to the imports available for annotations only
        /// </summary>
        /// <param name="autoImport">class to add</param>
        public void AddAnnotationImport(Type autoImport)
        {
            AddAnnotationImport(autoImport.Name);
        }

        /// <summary>
        ///     Remove an import.
        /// </summary>
        /// <param name="name">to remove</param>
        public void RemoveImport(string name)
        {
            imports.Remove(name);
        }

        /// <summary>
        ///     Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="className">is the class name (simple or fully-qualified) providing methods</param>
        /// <param name="methodInvocationConfig">is the cache configuration</param>
        public void AddMethodRef(
            string className,
            ConfigurationCommonMethodRef methodInvocationConfig)
        {
            methodInvocationReferences.Put(className, methodInvocationConfig);
        }

        /// <summary>
        ///     Adds a cache configuration for a class providing methods for use in the from-clause.
        /// </summary>
        /// <param name="clazz">is the class providing methods</param>
        /// <param name="methodInvocationConfig">is the cache configuration</param>
        public void AddMethodRef(
            Type clazz,
            ConfigurationCommonMethodRef methodInvocationConfig)
        {
            methodInvocationReferences.Put(clazz.Name, methodInvocationConfig);
        }

        /// <summary>
        ///     Add a global variable.
        ///     <para />
        ///     Use the runtime API to set variable values or EPL statements to change variable values.
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any value or
        ///     an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned value.For static initialization the value can be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be Serializable.
        /// </param>
        /// <throws>
        ///     ConfigurationException if the type and initialization value don't match or the variable name is already in use
        /// </throws>
        public void AddVariable(
            string variableName,
            Type type,
            object initializationValue)
        {
            AddVariable(variableName, type.Name, initializationValue, false);
        }

        /// <summary>
        ///     Add variable that can be a constant.
        /// </summary>
        /// <param name="variableName">name of variable</param>
        /// <param name="type">variable type</param>
        /// <param name="initializationValue">initial value</param>
        /// <param name="constant">constant indicator</param>
        public void AddVariable(
            string variableName,
            Type type,
            object initializationValue,
            bool constant)
        {
            AddVariable(variableName, type.Name, initializationValue, constant);
        }

        /// <summary>
        ///     Add a global variable.
        ///     <para />
        ///     Use the runtime API to set variable values or EPL statements to change variable values.
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any value or
        ///     an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned valueThe value can be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be Serializable.
        /// </param>
        /// <throws>
        ///     ConfigurationException if the type and initialization value don't match or the variable name is already in use
        /// </throws>
        public void AddVariable(
            string variableName,
            string type,
            object initializationValue)
        {
            AddVariable(variableName, type, initializationValue, false);
        }

        /// <summary>
        ///     Add a global variable, allowing constants.
        ///     <para />
        ///     Use the runtime API to set variable values or EPL statements to change variable values.
        /// </summary>
        /// <param name="variableName">name of the variable to add</param>
        /// <param name="type">
        ///     the type name of the variable, must be a primitive or boxed builtin scalar type or "object" for any
        ///     value or an event type name or a class name or fully-qualified class name.  Append "[]" for array.
        /// </param>
        /// <param name="initializationValue">
        ///     is the first assigned valueFor static initialization the value can be string-typed and will be parsed.
        ///     For static initialization the initialization value, if provided, must be Serializable />.
        /// </param>
        /// <param name="constant">true to identify the variable as a constant</param>
        /// <throws>
        ///     ConfigurationException if the type and initialization value don't match or the variable name is already in use
        /// </throws>
        public void AddVariable(
            string variableName,
            string type,
            object initializationValue,
            bool constant)
        {
            var configVar = new ConfigurationCommonVariable();
            configVar.VariableType = type;
            configVar.InitializationValue = initializationValue;
            configVar.IsConstant = constant;
            variables.Put(variableName, configVar);
        }

        /// <summary>
        ///     Adds a new variant stream. Variant streams allow events of disparate types to be treated the same.
        /// </summary>
        /// <param name="variantStreamName">is the name of the variant stream</param>
        /// <param name="variantStreamConfig">the configuration such as variant type names and any-type setting</param>
        public void AddVariantStream(
            string variantStreamName,
            ConfigurationCommonVariantStream variantStreamConfig)
        {
            variantStreams.Put(variantStreamName, variantStreamConfig);
        }

        /// <summary>
        ///     Returns true if a variant stream by the name has been declared, or false if not.
        /// </summary>
        /// <param name="name">of variant stream</param>
        /// <returns>indicator whether the variant stream by that name exists</returns>
        public bool IsVariantStreamExists(string name)
        {
            return variantStreams.ContainsKey(name);
        }

        /// <summary>
        ///     Adds an Avro event type
        /// </summary>
        /// <param name="eventTypeName">type name</param>
        /// <param name="avro">configs</param>
        public void AddEventTypeAvro(
            string eventTypeName,
            ConfigurationCommonEventTypeAvro avro)
        {
            eventTypesAvro.Put(eventTypeName, avro);
        }

        /// <summary>
        ///     Reset to an empty configuration.
        /// </summary>
        protected void Reset()
        {
            eventClasses = new LinkedHashMap<string, string>();
            mapNames = new LinkedHashMap<string, Properties>();
            nestableMapNames = new LinkedHashMap<string, IDictionary<string, object>>();
            nestableObjectArrayNames = new LinkedHashMap<string, IDictionary<string, object>>();
            eventTypesXMLDOM = new LinkedHashMap<string, ConfigurationCommonEventTypeXMLDOM>();
            eventTypesAvro = new LinkedHashMap<string, ConfigurationCommonEventTypeAvro>();
            eventTypesBean = new LinkedHashMap<string, ConfigurationCommonEventTypeBean>();
            databaseReferences = new Dictionary<string, ConfigurationCommonDBRef>();
            imports = new List<string>();
            annotationImports = new List<string>();
            AddDefaultImports();
            variables = new Dictionary<string, ConfigurationCommonVariable>();
            methodInvocationReferences = new Dictionary<string, ConfigurationCommonMethodRef>();
            variantStreams = new Dictionary<string, ConfigurationCommonVariantStream>();
            mapTypeConfigurations = new Dictionary<string, ConfigurationCommonEventTypeMap>();
            objectArrayTypeConfigurations = new Dictionary<string, ConfigurationCommonEventTypeObjectArray>();
            eventMeta = new ConfigurationCommonEventTypeMeta();
            logging = new ConfigurationCommonLogging();
            timeSource = new ConfigurationCommonTimeSource();
            transientConfiguration = new Dictionary<string, object>(2);
            eventTypeAutoNamePackages = new HashSet<string>();
            execution = new ConfigurationCommonExecution();
        }

        /// <summary>
        ///     Adds a namespace of a package that event classes reside in.
        ///     <para />
        ///     This setting allows an application to place all it's events into one or more namespaces
        ///     and then declare these packages via this method. The runtime
        ///     attempts to resolve an event type name to a type residing in each declared package.
        ///     <para />
        ///     For example, in the statement "select * from MyEvent" the runtime attempts to load class "namespace.MyEvent"
        ///     and if successful, uses that class as the event type.
        /// </summary>
        /// <param name="packageName">is the fully-qualified namespace of the namespace that event classes reside in</param>
        public void AddEventTypeAutoName(string packageName)
        {
            eventTypeAutoNamePackages.Add(packageName);
        }

        /// <summary>
        ///     Use these imports until the user specifies something else.
        /// </summary>
        private void AddDefaultImports()
        {
            imports.Add("java.lang.*");
            imports.Add("java.math.*");
            imports.Add("java.text.*");
            imports.Add("java.util.*");
            imports.Add(ANNOTATION_IMPORT);
            imports.Add(DATAFLOWOPERATOR_IMPORT);
        }
    }
} // end of namespace