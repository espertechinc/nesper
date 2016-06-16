///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;

namespace com.espertech.esper.client
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Provides configurations for an engine instance.
    /// </summary>
    public interface ConfigurationInformation
    {
        /// <summary>
        /// Returns the service context factory class name
        /// </summary>
        /// <returns>
        /// class name
        /// </returns>
        string EPServicesContextFactoryClassName { get; }

        /// <summary>
        /// Returns the mapping of event type name to type name.
        /// </summary>
        /// <returns>
        /// event type names for type names
        /// </returns>
        IDictionary<string, string> EventTypeNames { get; }

        /// <summary>
        /// Returns a map keyed by event type name, and values being the definition for the
        /// event type of the property names and types that make up the event.
        /// </summary>
        /// <returns>
        /// map of event type name and definition of event properties
        /// </returns>
        IDictionary<string, Properties> EventTypesMapEvents { get; }

        /// <summary>
        /// Returns a map keyed by event type name, and values being the definition for the
        /// event type of the property names and types that make up the event, for nestable,
        /// strongly-typed Map-based event representations.
        /// </summary>
        /// <returns>
        /// map of event type name and definition of event properties
        /// </returns>
        IDictionary<string, DataMap> EventTypesNestableMapEvents { get; }

        /// <summary>
        /// Returns the mapping of event type name to XML DOM event type information.
        /// </summary>
        /// <returns>
        /// event type name mapping to XML DOM configs
        /// </returns>
        IDictionary<string, ConfigurationEventTypeXMLDOM> EventTypesXMLDOM { get; }

        /// <summary>
        /// Returns the mapping of event type name to legacy java event type information.
        /// </summary>
        /// <returns>
        /// event type name mapping to legacy java class configs
        /// </returns>
        IDictionary<string, ConfigurationEventTypeLegacy> EventTypesLegacy { get; }

        /// <summary>
        /// Returns the import information for types.
        /// </summary>
        /// <returns>
        /// imported names
        /// </returns>
        IList<AutoImportDesc> Imports { get; }

        /// <summary>
        /// Returns the import information for annotations.
        /// </summary>
        /// <value>
        /// The annotation imports.
        /// </value>
        IList<AutoImportDesc> AnnotationImports { get; }

        /// <summary>
        /// Returns a map of string database names to database configuration options.
        /// </summary>
        /// <returns>
        /// map of database configurations
        /// </returns>
        IDictionary<string, ConfigurationDBRef> DatabaseReferences { get; }

        /// <summary>
        /// Returns a list of configured plug-in views.
        /// </summary>
        /// <returns>
        /// list of plug-in view configs
        /// </returns>
        IList<ConfigurationPlugInView> PlugInViews { get; }

        /// <summary>
        /// Returns a list of configured plug-in virtual data windows
        /// </summary>
        IList<ConfigurationPlugInVirtualDataWindow> PlugInVirtualDataWindows { get; }

        /// <summary>
        /// Returns a list of configured plugin loaders.
        /// </summary>
        /// <returns>
        /// adapter loaders
        /// </returns>
        IList<ConfigurationPluginLoader> PluginLoaders { get; }

        /// <summary>
        /// Returns a list of configured plug-in aggregation functions.
        /// </summary>
        /// <returns>
        /// list of configured aggregations
        /// </returns>
        IList<ConfigurationPlugInAggregationFunction> PlugInAggregationFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-in multi-function aggregation functions.
        /// </summary>
        /// <returns>
        /// list of configured multi-function aggregations
        /// </returns>
        IList<ConfigurationPlugInAggregationMultiFunction> PlugInAggregationMultiFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-in single-row functions.
        /// </summary>
        /// <value>The plug in single row functions.</value>
        /// <returns>list of configured single-row functions</returns>
        IList<ConfigurationPlugInSingleRowFunction> PlugInSingleRowFunctions { get; }

        /// <summary>
        /// Returns a list of configured plug-ins for pattern observers and guards.
        /// </summary>
        /// <returns>
        /// list of pattern plug-ins
        /// </returns>
        IList<ConfigurationPlugInPatternObject> PlugInPatternObjects { get; }

        /// <summary>
        /// Returns engine default settings.
        /// </summary>
        /// <returns>
        /// engine defaults
        /// </returns>
        ConfigurationEngineDefaults EngineDefaults { get; }

        /// <summary>
        /// Returns the global variables by name as key and type plus initialization value as value
        /// </summary>
        /// <returns>
        /// map of variable name and variable configuration
        /// </returns>
        IDictionary<string, ConfigurationVariable> Variables { get; }

        /// <summary>
        /// Returns a map of class name and cache configurations, for use in method
        /// invocations in the from-clause of methods provided by the class.
        /// </summary>
        /// <returns>
        /// map of fully-qualified or simple class name and cache configuration
        /// </returns>
        IDictionary<string, ConfigurationMethodRef> MethodInvocationReferences { get; }

        /// <summary>
        /// Returns a set of namespaces that event classes reside in.
        /// <para/>
        /// This setting allows an application to place all it's events into one or more
        /// namespaces and then declare these namespaces via this method. The engine attempts
        /// to resolve an event type name to a class residing in each declared namespace.
        /// <para/>
        /// For example, in the statement "select * from MyEvent" the engine attempts to
        /// load class "namespace.MyEvent" and if successful, uses that class as the
        /// event type.
        /// </summary>
        /// <returns>
        /// set of namespace to look for events types when encountering a new event
        /// type name
        /// </returns>
        ICollection<string> EventTypeAutoNamePackages { get; }

        /// <summary>
        /// Returns a map of plug-in event representation URI and their event representation
        /// class and initializer.
        /// </summary>
        /// <returns>
        /// map of URI keys and event representation configuration
        /// </returns>
        IDictionary<Uri, ConfigurationPlugInEventRepresentation> PlugInEventRepresentation { get; }

        /// <summary>
        /// Returns a map of event type name of those event types that will be supplied by a
        /// plug-in event representation, and their configuration.
        /// </summary>
        /// <returns>
        /// map of names to plug-in event type config
        /// </returns>
        IDictionary<string, ConfigurationPlugInEventType> PlugInEventTypes { get; }

        /// <summary>
        /// Returns the URIs that point to plug-in event representations that are given a
        /// chance to dynamically resolve an event type name to an event type, when a new
        /// (unseen) event type name occurs in a new EPL statement.
        /// <para/>
        /// The order of the URIs matters as event representations are asked in turn, to
        /// accept the name.
        /// <para/>
        /// URIs can be child URIs of plug-in event representations and can add additional
        /// parameters or fragments for use by the event representation.
        /// </summary>
        /// <returns>
        /// URIs for resolving an event type name
        /// </returns>
        IList<Uri> PlugInEventTypeResolutionURIs { get; }

        /// <summary>
        /// Returns a map of revision event type name and revision event type configuration.
        /// Revision event types handle updates (new versions) for past events.
        /// </summary>
        /// <returns>
        /// map of name and revision event type config
        /// </returns>
        IDictionary<string, ConfigurationRevisionEventType> RevisionEventTypes { get; }

        /// <summary>
        /// Returns a map of variant stream name and variant configuration information.
        /// Variant streams allows handling events of all sorts of different event types the same
        /// way.
        /// </summary>
        /// <returns>
        /// map of name and variant stream config
        /// </returns>
        IDictionary<string, ConfigurationVariantStream> VariantStreams { get; }

        /// <summary>
        /// Returns for each Map event type name the set of supertype event type names (Map
        /// types only).
        /// </summary>
        /// <returns>
        /// map of name to set of supertype names
        /// </returns>
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
        IDictionary<string, DataMap> EventTypesNestableObjectArrayEvents { get; }
    }
}
