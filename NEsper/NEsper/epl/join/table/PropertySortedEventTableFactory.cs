///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex that organizes events by the event property values into a single TreeMap sortable 
    /// non-nested index with Object keys that store the property values.
    /// </summary>
    public class PropertySortedEventTableFactory : EventTableFactory
    {
        protected readonly int StreamNum;
        protected readonly String PropertyName;
    
        /// <summary>Getters for properties. </summary>
        protected readonly EventPropertyGetter PropertyGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="propertyName">Name of the property.</param>
        public PropertySortedEventTableFactory(int streamNum, EventType eventType, String propertyName)
        {
            StreamNum = streamNum;
            PropertyName = propertyName;
            PropertyGetter = EventBeanUtility.GetAssertPropertyGetter(eventType, propertyName);
        }

        public virtual EventTable[] MakeEventTables()
        {
            var organization = new EventTableOrganization(null, false, false, StreamNum, new String[] {PropertyName}, EventTableOrganization.EventTableOrganizationType.BTREE);
            return new EventTable[]
            {
                new PropertySortedEventTable(PropertyGetter, organization)
            };
        }

        public virtual Type EventTableType
        {
            get { return typeof (PropertySortedEventTable); }
        }

        public virtual String ToQueryPlan()
        {
            return GetType().FullName +
                    " streamNum=" + StreamNum +
                    " propertyName=" + PropertyName;
        }
    }
}
