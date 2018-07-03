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
    /// <summary>Provides configurations for an engine instance.</summary>
    public interface ConfigurationInformation
    {
        /// <summary>
        /// Returns the service context factory class name
        /// </summary>
        /// <value>class name</value>
        string EPServicesContextFactoryClassName { get; }

        /// <summary>
        /// Returns the mapping of event type name to class name.
        /// </summary>
        /// <value>event type names for Java class names</value>
        IDictionary<string, string> EventTypeNames { get; }

        /// <summary>
        /// Returns a map keyed by event type name, and values being the definition for the
        /// Map event type of the property names and types that make up the event.
        /// </summary>
        /// <value>map of event type name and definition of event properties</value>
        IDictionary<string, Properties> EventTypesMapEvents { get; }

        /// <summary>
        /// Returns a map keyed by event type name, and values being the definition for the
        /// event type of the property names and types that make up the event,
        /// for nestable, strongly-typed Map-based event representations.
        /// </summary>
        /// <value>map of event type name and definition of event properties</value>
        IDictionary<string, IDictionary<string, object>> EventTypesNestableMapEvents { get; }

        /// <summary>
        /// Returns the mapping of event type name to XML DOM event type information.
        /// </summary>
        /// <value>event type name mapping to XML DOM configs</value>
        IDictionary<string, ConfigurationEventTypeXMLDOM> EventTypesXMLDOM { get; }

        /// <summary>
        /// Returns the Avro event types.
        /// </summary>
        /// <value>Avro event types</value>
        IDictionary<string, ConfigurationEventTypeAvro> EventTypesAvro { get; }

        /// <summary>
        /// Returns the mapping of event type name to legacy java event type information.
        /// </summary>
        /// <value>event type name mapping to legacy java class configs</value>
        IDictionary<string, ConfigurationEventTypeLegacy> EventTypesLegacy { get; }

        /// <summary>
        /// Returns the class and package imports.
        /// </summary>
        /// <value>imported names</value>
        IList<AutoImportDesc> Imports { get; }

        /// <summary>
        /// Returns the class and package imports for annotation-only use.
        /// </summary>
        /// <value>imported names</value>
        IList<AutoImportDesc> AnnotationImports { get; }

        /// <summary>
        /// Returns a map of string database names to database configuration options.
        /// </summary>
        /// <value>map of database configurations</value>
        IDictionary<string, ConfigurationDBRef> DatabaseReferences { get; }

        /// <summary>
        /// Returns a list of configured plug-in views.
        /// </summary>
        /// <value>list of plug-in view configs</value>
        IList<ConfigurationPlugInView> PlugInViews { get; }

        /// <summary>
        /// Returns a list of configured plug-in virtual data windows.
        /// </summary>
        /// <value>list of plug-in virtual data windows</value>
        IList<ConfigurationPlugInVirtualDataWindow> PlugInVirtualDataWindows { get; }

        /// <summary>
        /// Returns a list of configured plugin loaders.
        /// </summary>
        /// <value>adapter loaders</value>
        IList<ConfigurationPluginLoader> PluginLoaders { get; }

        /// <summary>
        /// Returns a list of configured plug-in aggregation functions.
        /// </summary>
        /// <value>list of configured aggregations</value>
        IList<ConfigurationPlugInAggregationFunction> PlugInAggregationFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-in multi-function aggregation functions.
        /// </summary>
        /// <value>list of configured multi-function aggregations</value>
        IList<ConfigurationPlugInAggregationMultiFunction> PlugInAggregationMultiFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-in single-row functions.
        /// </summary>
        /// <value>list of configured single-row functions</value>
        IList<ConfigurationPlugInSingleRowFunction> PlugInSingleRowFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-ins for pattern observers and guards.
        /// </summary>
        /// <value>list of pattern plug-ins</value>
        IList<ConfigurationPlugInPatternObject> PlugInPatternObjects { get; }

        /// <summary>
        /// Returns engine default settings.
        /// </summary>
        /// <value>engine defaults</value>
        ConfigurationEngineDefaults EngineDefaults { get; }

        /// <summary>
        /// Returns the global variables by name as key and type plus initialization value as value
        /// </summary>
        /// <value>map of variable name and variable configuration</value>
        IDictionary<string, ConfigurationVariable> Variables { get; }

        /// <summary>
        /// Returns a map of class name and cache configurations, for use in
        /// method invocations in the from-clause of methods provided by the class.
        /// </summary>
        /// <value>
        ///   map of fully-qualified or simple class name and cache configuration
        /// </value>
        IDictionary<string, ConfigurationMethodRef> MethodInvocationReferences { get; }

        /// <summary>
        /// Returns a set of namespace names that event classes reside in.
        /// <para>
        /// This setting allows an application to place all it's events into one or more namespaces
        /// and then declare these packages via this method. The engine
        /// attempts to resolve an event type name to a class residing in each declared package.
        /// </para>
        /// <para>
        /// For example, in the statement "select * from MyEvent" the engine attempts to load class "javaPackageName.MyEvent"
        /// and if successful, uses that class as the event type.
        /// </para>
        /// </summary>
        /// <value>
        ///   set of namespaces to look for events types when encountering a new event type name
        /// </value>
        ISet<string> EventTypeAutoNamePackages { get; }

        /// <summary>
        /// Returns a map of plug-in event representation URI and their event representation class and initializer.
        /// </summary>
        /// <value>map of URI keys and event representation configuration</value>
        IDictionary<Uri, ConfigurationPlugInEventRepresentation> PlugInEventRepresentation { get; }

        /// <summary>
        /// Returns a map of event type name of those event types that will be supplied by a plug-in event representation,
        /// and their configuration.
        /// </summary>
        /// <value>map of names to plug-in event type config</value>
        IDictionary<string, ConfigurationPlugInEventType> PlugInEventTypes { get; }

        /// <summary>
        /// Returns the URIs that point to plug-in event representations that are given a chance to dynamically resolve an event
        /// type name to an event type, when a new (unseen) event type name occurs in a new EPL statement.
        /// <para>
        /// The order of the URIs matters as event representations are asked in turn, to accept the name.
        /// </para>
        /// <para>
        /// URIs can be child URIs of plug-in event representations and can add additional parameters or fragments
        /// for use by the event representation.
        /// </para>
        /// </summary>
        /// <value>URIs for resolving an event type name</value>
        IList<Uri> PlugInEventTypeResolutionURIs { get; }

        /// <summary>
        /// Returns a map of revision event type name and revision event type configuration. Revision event types handle updates (new versions)
        /// for past events.
        /// </summary>
        /// <value>map of name and revision event type config</value>
        IDictionary<string, ConfigurationRevisionEventType> RevisionEventTypes { get; }

        /// <summary>
        /// Returns a map of variant stream name and variant configuration information. Variant streams allows handling
        /// events of all sorts of different event types the same way.
        /// </summary>
        /// <value>map of name and variant stream config</value>
        IDictionary<string, ConfigurationVariantStream> VariantStreams { get; }

        /// <summary>
        /// Returns for each Map event type name the set of supertype event type names (Map types only).
        /// </summary>
        /// <value>map of name to set of supertype names</value>
        IDictionary<string, ConfigurationEventTypeMap> MapTypeConfigurations { get; }

        /// <summary>
        /// Returns the object-array event type configurations.
        /// </summary>
        /// <value>type configs</value>
        IDictionary<string, ConfigurationEventTypeObjectArray> ObjectArrayTypeConfigurations { get; }

        /// <summary>
        /// Returns the object-array event types.
        /// </summary>
        /// <value>object-array event types</value>
        IDictionary<string, IDictionary<string, object>> EventTypesNestableObjectArrayEvents { get; }

        /// <summary>
        /// Returns the transient configuration, which are configuration values that are passed by reference (and not by value)
        /// </summary>
        /// <value>transient configuration</value>
        IDictionary<string, object> TransientConfiguration { get; }
    }
} // end of namespace
