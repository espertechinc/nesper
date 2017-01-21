///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.vaevent
{
    /// <summary>
    /// Specification for how to build a revision event type.
    /// <para/>
    /// Compiled from the information provided via configuration, which has already been validated
    /// before building this specification.
    /// </summary>
    public class RevisionSpec
    {
        /// <summary>Ctor. </summary>
        /// <param name="propertyRevision">strategy to use</param>
        /// <param name="baseEventType">base type</param>
        /// <param name="deltaTypes">delta types</param>
        /// <param name="deltaNames">aliases of delta types</param>
        /// <param name="keyPropertyNames">names of key properties</param>
        /// <param name="changesetPropertyNames">names of properties that change</param>
        /// <param name="baseEventOnlyPropertyNames">properties only available on the base event</param>
        /// <param name="deltaTypesAddProperties">bool to indicate delta types add additional properties.</param>
        /// <param name="changesetPropertyDeltaContributed">flag for each property indicating whether its contributed only by a delta event</param>
        public RevisionSpec(PropertyRevisionEnum propertyRevision,
                            EventType baseEventType,
                            EventType[] deltaTypes,
                            String[] deltaNames,
                            String[] keyPropertyNames,
                            String[] changesetPropertyNames,
                            String[] baseEventOnlyPropertyNames,
                            bool deltaTypesAddProperties,
                            bool[] changesetPropertyDeltaContributed)
        {
            PropertyRevision = propertyRevision;
            BaseEventType = baseEventType;
            DeltaTypes = deltaTypes;
            DeltaNames = deltaNames;
            KeyPropertyNames = keyPropertyNames;
            ChangesetPropertyNames = changesetPropertyNames;
            BaseEventOnlyPropertyNames = baseEventOnlyPropertyNames;
            IsDeltaTypesAddProperties = deltaTypesAddProperties;
            ChangesetPropertyDeltaContributed = changesetPropertyDeltaContributed;
        }

        /// <summary>Flag for each changeset property to indicate if only the delta contributes the property. </summary>
        /// <returns>flag per property</returns>
        public bool[] ChangesetPropertyDeltaContributed { get; private set; }

        /// <summary>Returns the stratgegy for revisioning. </summary>
        /// <returns>enum</returns>
        public PropertyRevisionEnum PropertyRevision { get; private set; }

        /// <summary>Returns the base event type. </summary>
        /// <returns>base type</returns>
        public EventType BaseEventType { get; private set; }

        /// <summary>Returns the delta event types. </summary>
        /// <returns>types</returns>
        public EventType[] DeltaTypes { get; private set; }

        /// <summary>Returns aliases for delta events. </summary>
        /// <returns>event type alias names for delta events</returns>
        public string[] DeltaNames { get; private set; }

        /// <summary>Returns property names for key properties. </summary>
        /// <returns>property names</returns>
        public string[] KeyPropertyNames { get; private set; }

        /// <summary>Returns property names of properties that change by deltas </summary>
        /// <returns>prop names</returns>
        public string[] ChangesetPropertyNames { get; private set; }

        /// <summary>Returns the properies only found on the base event. </summary>
        /// <returns>base props</returns>
        public string[] BaseEventOnlyPropertyNames { get; private set; }

        /// <summary>Returns true if delta types add properties. </summary>
        /// <returns>flag indicating if delta event types add properties</returns>
        public bool IsDeltaTypesAddProperties { get; private set; }
    }
}
