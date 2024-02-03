///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.util
{
    /// <summary>
    ///     Pair of event type and property.
    /// </summary>
    public class EventTypePropertyPair
    {
        private readonly EventType eventType;
        private readonly string propertyName;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="propertyName">property</param>
        public EventTypePropertyPair(
            EventType eventType,
            string propertyName)
        {
            this.eventType = eventType;
            this.propertyName = propertyName;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (EventTypePropertyPair)o;
            if (!eventType.Equals(that.eventType)) {
                return false;
            }

            if (!propertyName.Equals(that.propertyName)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return CompatExtensions.HashAll<object>(propertyName, eventType);
        }
    }
} // end of namespace