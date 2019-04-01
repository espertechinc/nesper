///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Index factory that organizes events by the event property values into hash buckets. 
    /// Based on a Dictionary with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped" />
    /// keys that store the property values.
    /// </summary>
    public class PropertyIndexedEventTableSingleArrayFactory : EventTableFactory
    {
        protected readonly int StreamNum;
        protected readonly String[] PropertyNames;
        protected readonly bool Unique;
        protected readonly String OptionalIndexName;

        protected readonly EventPropertyGetter[] PropertyGetters;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="propertyNames">The property names.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="optionalIndexName">Name of the optional index.</param>
        public PropertyIndexedEventTableSingleArrayFactory(int streamNum, EventType eventType, String[] propertyNames, bool unique, String optionalIndexName)
        {
            StreamNum = streamNum;
            PropertyNames = propertyNames;
            Unique = unique;
            OptionalIndexName = optionalIndexName;

            // Init getters
            PropertyGetters = new EventPropertyGetter[propertyNames.Length];
            for (var i = 0; i < propertyNames.Length; i++)
            {
                PropertyGetters[i] = EventBeanUtility.GetAssertPropertyGetter(eventType, propertyNames[i]);
            }
        }

        public EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var tables = new EventTable[PropertyGetters.Length];
            if (Unique)
            {
                for (var i = 0; i < tables.Length; i++)
                {
                    var organization = new EventTableOrganization(OptionalIndexName, Unique, false, StreamNum, new String[] { PropertyNames[i] }, EventTableOrganizationType.HASH);
                    tables[i] = new PropertyIndexedEventTableSingleUnique(PropertyGetters[i], organization);
                }
            }
            else
            {
                for (var i = 0; i < tables.Length; i++)
                {
                    var organization = new EventTableOrganization(OptionalIndexName, Unique, false, StreamNum, new String[] { PropertyNames[i] }, EventTableOrganizationType.HASH);
                    tables[i] = new PropertyIndexedEventTableSingleUnadorned(PropertyGetters[i], organization);
                }
            }
            return tables;
        }

        public Type EventTableType
        {
            get
            {
                if (Unique)
                {
                    return typeof(PropertyIndexedEventTableSingleUnique);
                }
                else
                {
                    return typeof(PropertyIndexedEventTableSingle);
                }
            }
        }

        public String ToQueryPlan()
        {
            return GetType().Name +
                    (Unique ? " unique" : " non-unique") +
                    " streamNum=" + StreamNum +
                    " propertyNames=" + PropertyNames.Render();
        }
    }
}
