///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;

namespace com.espertech.esper.common.client.serde
{
    /// <summary>
    ///     Information about the event property for which to obtain a serde.
    /// </summary>
    public class SerdeProviderAdditionalInfoEventProperty : SerdeProviderAdditionalInfo
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="raw">statement information</param>
        /// <param name="eventTypeName">event type name</param>
        /// <param name="eventPropertyName">property name</param>
        public SerdeProviderAdditionalInfoEventProperty(
            StatementRawInfo raw,
            string eventTypeName,
            string eventPropertyName) : base(raw)
        {
            EventTypeName = eventTypeName;
            EventPropertyName = eventPropertyName;
        }

        /// <summary>
        ///     Returns the event property name
        /// </summary>
        /// <value>event property name</value>
        public string EventPropertyName { get; set; }

        /// <summary>
        ///     Returns the event type name
        /// </summary>
        /// <value>event type name</value>
        public string EventTypeName { get; set; }

        public override string ToString()
        {
            return "event-type '" +
                   EventTypeName +
                   '\'' +
                   " property '" +
                   EventPropertyName +
                   '\'';
        }
    }
} // end of namespace