///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     Information about the event type for which to obtain a serde.
    /// </summary>
    public class SerdeProviderAdditionalInfoEventType : SerdeProviderAdditionalInfo
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="raw">statement info</param>
        /// <param name="eventTypeName">event type name</param>
        /// <param name="eventTypeSupertypes">optional supertypes</param>
        public SerdeProviderAdditionalInfoEventType(
            StatementRawInfo raw,
            string eventTypeName,
            IList<EventType> eventTypeSupertypes) : base(raw)
        {
            EventTypeName = eventTypeName;
            EventTypeSupertypes = eventTypeSupertypes;
        }

        /// <summary>
        ///     Returns the event type name if provided
        /// </summary>
        /// <value>type name</value>
        public string EventTypeName { get; }

        /// <summary>
        ///     Returns supertypes when available.
        /// </summary>
        /// <value>supertypes</value>
        public IList<EventType> EventTypeSupertypes { get; }

        public override string ToString()
        {
            return "event-type '" + EventTypeName + '\'';
        }
    }
} // end of namespace