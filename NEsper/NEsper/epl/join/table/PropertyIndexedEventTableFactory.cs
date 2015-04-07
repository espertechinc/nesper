///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// MapIndex factory that organizes events by the event property values into hash buckets. 
    /// Based on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> 
    /// keys that store the property values. Takes a list of property names as parameter. 
    /// Doesn't care which event type the events have as long as the properties exist. If 
    /// the same event is added twice, the class throws an exception on add.
    /// </summary>
    public class PropertyIndexedEventTableFactory : EventTableFactory
    {
        protected readonly int StreamNum;
        protected readonly String[] PropertyNames;
        protected readonly bool Unique;
        protected readonly String OptionalIndexName;
    
        /// <summary>Getters for properties. </summary>
        protected readonly EventPropertyGetter[] PropertyGetters;
    
        /// <summary>Ctor. </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="propertyNames">property names to use for indexing</param>
        /// <param name="unique"></param>
        /// <param name="optionalIndexName"></param>
        public PropertyIndexedEventTableFactory(int streamNum, EventType eventType, IList<String> propertyNames, bool unique, String optionalIndexName)
        {
            StreamNum = streamNum;
            PropertyNames = propertyNames.ToArray();
            Unique = unique;
            OptionalIndexName = optionalIndexName;
    
            // Init getters
            PropertyGetters = new EventPropertyGetter[propertyNames.Count];
            for (int i = 0; i < propertyNames.Count; i++)
            {
                PropertyGetters[i] = eventType.GetGetter(propertyNames[i]);
            }
        }
    
        public virtual EventTable[] MakeEventTables()
        {
            var organization = new EventTableOrganization(OptionalIndexName, Unique, false,
                    StreamNum, PropertyNames, EventTableOrganization.EventTableOrganizationType.HASH);
            var eventTable = Unique
                ? new PropertyIndexedEventTableUnique(PropertyGetters, organization)
                : new PropertyIndexedEventTable(PropertyGetters, organization);
            return new EventTable[]
            {
                eventTable
            };
        }

        public virtual Type EventTableType
        {
            get
            {
                if (Unique)
                {
                    return typeof (PropertyIndexedEventTableUnique);
                }
                else
                {
                    return typeof (PropertyIndexedEventTable);
                }
            }
        }

        public virtual string ToQueryPlan()
        {
            return GetType().FullName +
                    (Unique ? " unique" : " non-unique") +
                    " streamNum=" + StreamNum +
                    " propertyNames=" + PropertyNames.Render();
        }
    }
}
