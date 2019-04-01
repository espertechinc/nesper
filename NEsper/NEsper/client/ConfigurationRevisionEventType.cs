///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Configuration information for revision event types.
    /// <para/>
    /// The configuration information consists of the names of the base event type and
    /// the delta event types, as well as the names of properties that supply key values,
    /// and a strategy.
    /// <para/>
    /// Events of the base event type arrive before delta events; Delta events arriving
    /// before the base event for the same key value are not processed, as delta events
    /// as well as base events represent new versions.
    /// </summary>
    [Serializable]
    public class ConfigurationRevisionEventType
    {
        private readonly ICollection<String> nameBaseEventTypes;
        private readonly ICollection<String> nameDeltaEventTypes;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public ConfigurationRevisionEventType()
        {
            nameBaseEventTypes = new HashSet<String>();
            nameDeltaEventTypes = new HashSet<String>();
            PropertyRevision = PropertyRevisionEnum.OVERLAY_DECLARED;
        }
    
        /// <summary>
        /// Add a base event type by it's name.
        /// </summary>
        /// <param name="nameBaseEventType">the name of the base event type to add</param>
        public void AddNameBaseEventType(String nameBaseEventType)
        {
            nameBaseEventTypes.Add(nameBaseEventType);
        }

        /// <summary>
        /// Returns the set of event type names that are base event types.
        /// </summary>
        /// <returns>
        /// names of base event types
        /// </returns>
        public ICollection<string> NameBaseEventTypes
        {
            get { return nameBaseEventTypes; }
        }

        /// <summary>
        /// Returns the set of names of delta event types.
        /// </summary>
        /// <returns>
        /// names of delta event types
        /// </returns>
        public ICollection<string> NameDeltaEventTypes
        {
            get { return nameDeltaEventTypes; }
        }

        /// <summary>
        /// Add a delta event type by it's name.
        /// </summary>
        /// <param name="nameDeltaEventType">the name of the delta event type to add</param>
        public void AddNameDeltaEventType(String nameDeltaEventType)
        {
            nameDeltaEventTypes.Add(nameDeltaEventType);
        }

        /// <summary>
        /// Returns the enumeration value defining the strategy to use for overlay or
        /// merging multiple versions of an event (instances of base and delta events).
        /// </summary>
        /// <returns>
        /// strategy enumerator
        /// </returns>
        public PropertyRevisionEnum PropertyRevision { get; set; }

        /// <summary>
        /// Returns the key property names, which are the names of the properties that
        /// supply key values for relating base and delta events.
        /// </summary>
        /// <value>
        /// 	array of names of key properties
        /// </value>
        public string[] KeyPropertyNames { get; set; }
    }

    /// <summary>
    /// Enumeration for specifying a strategy to use to merge or overlay properties.
    /// </summary>
    public enum PropertyRevisionEnum
    {
        /// <summary>
        /// A fast strategy for revising events that groups properties provided by base and
        /// delta events and overlays contributed properties to compute a revision.
        /// <para/>
        /// For use when there is a limited number of combinations of properties that
        /// change on an event, and such combinations are known in advance.
        /// <para/>
        /// The properties available on the output revision events are all properties of
        /// the base event type. Delta event types do not add any additional properties that
        /// are not present on the base event type.
        /// <para/>
        /// Any null values or non-existing property on a delta (or base) event results in
        /// a null values for the same property on the output revision event.
        /// </summary>
        OVERLAY_DECLARED,

        /// <summary>
        /// A strategy for revising events by merging properties provided by base and delta
        /// events, considering null values and non-existing (dynamic) properties as well.
        /// <para/>
        /// For use when there is a limited number of combinations of properties that
        /// change on an event, and such combinations are known in advance.
        /// <para/>
        /// The properties available on the output revision events are all properties of
        /// the base event type plus all additional properties that any of the delta event
        /// types provide.
        /// <para/>
        /// Any null values or non-existing property on a delta (or base) event results in
        /// a null values for the same property on the output revision event.
        /// </summary>
        MERGE_DECLARED,

        /// <summary>
        /// A strategy for revising events by merging properties provided by base and delta
        /// events, considering only non-null values.
        /// <para/>
        /// For use when there is an unlimited number of combinations of properties that
        /// change on an event, or combinations are not known in advance.
        /// <para/>
        /// The properties available on the output revision events are all properties of
        /// the base event type plus all additional properties that any of the delta event
        /// types provide.
        /// <para/>
        /// Null values returned by delta (or base) event properties provide no value to
        /// output revision events, i.e. null values are not merged.
        /// </summary>
        MERGE_NON_NULL,

        /// <summary>
        /// A strategy for revising events by merging properties provided by base and delta
        /// events, considering only values supplied by event properties that exist.
        /// <para/>
        /// For use when there is an unlimited number of combinations of properties that
        /// change on an event, or combinations are not known in advance.
        /// <para/>
        /// The properties available on the output revision events are all properties of
        /// the base event type plus all additional properties that any of the delta event
        /// types provide.
        /// <para/>
        /// All properties are treated as dynamic properties: If an event property does not
        /// exist on a delta event (or base) event the property provides no value to output
        /// revision events, i.e. non-existing property values are not merged.
        /// </summary>
        MERGE_EXISTS
    }
}
