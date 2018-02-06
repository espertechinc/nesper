///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Index factory that organizes events by the event property values into hash buckets. 
    /// Based on a HashMap with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped"/> 
    /// keys that store the property values.
    /// </summary>
    public class PropertyIndexedEventTableSingleFactory : EventTableFactory
    {
        protected readonly int StreamNum;
        protected readonly String PropertyName;
        protected readonly bool Unique;
        protected readonly String OptionalIndexName;

        protected readonly EventPropertyGetter PropertyGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="optionalIndexName">Name of the optional index.</param>
        public PropertyIndexedEventTableSingleFactory(
            int streamNum,
            EventType eventType,
            String propertyName,
            bool unique,
            String optionalIndexName)
        {
            StreamNum = streamNum;
            PropertyName = propertyName;
            Unique = unique;
            OptionalIndexName = optionalIndexName;

            // Init getters
            PropertyGetter = EventBeanUtility.GetAssertPropertyGetter(eventType, propertyName);
        }

        public virtual EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var organization = new EventTableOrganization(OptionalIndexName, Unique, false, StreamNum, new String[] { PropertyName }, EventTableOrganizationType.HASH);
            if (Unique)
            {
                return new EventTable[] { new PropertyIndexedEventTableSingleUnique(PropertyGetter, organization) };
            }
            else
            {
                return new EventTable[] { new PropertyIndexedEventTableSingleUnadorned(PropertyGetter, organization) };
            }
        }

        public virtual Type EventTableType
        {
            get
            {
                return Unique
                    ? typeof(PropertyIndexedEventTableSingleUnique)
                    : typeof(PropertyIndexedEventTableSingle);
            }
        }

        public virtual String ToQueryPlan()
        {
            return GetType().Name +
                   (Unique ? " unique" : " non-unique") +
                   " streamNum=" + StreamNum +
                   " propertyName=" + PropertyName;
        }
    }
}
