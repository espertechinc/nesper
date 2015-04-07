///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.util
{
    /// <summary>
    /// Pair of event type and property.
    /// </summary>
    public class EventTypePropertyPair
    {
        private readonly String propertyName;
        private readonly EventType eventType;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">event type</param>
        /// <param name="propertyName">property</param>
        public EventTypePropertyPair(EventType eventType, String propertyName)
        {
            this.eventType = eventType;
            this.propertyName = propertyName;
        }

        public bool Equals(EventTypePropertyPair obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.propertyName, propertyName) && Equals(obj.eventType, eventType);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (EventTypePropertyPair)) return false;
            return Equals((EventTypePropertyPair) obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked {
                return ((propertyName != null ? propertyName.GetHashCode() : 0)*397) ^ (eventType != null ? eventType.GetHashCode() : 0);
            }
        }
    }
}
