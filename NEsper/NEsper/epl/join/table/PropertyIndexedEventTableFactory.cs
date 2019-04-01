///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Index factory that organizes events by the event property values into hash buckets. Based on a HashMap
    /// with <seealso cref="com.espertech.esper.collection.MultiKeyUntyped" /> keys that store the property values.
    /// Takes a list of property names as parameter. Doesn't care which event type the events have as long as the properties
    /// exist. If the same event is added twice, the class throws an exception on add.
    /// </summary>
    public class PropertyIndexedEventTableFactory : EventTableFactory
    {
        protected readonly int streamNum;
        protected readonly IList<string> propertyNames;
        protected readonly bool unique;
        protected readonly string optionalIndexName;

        /// <summary>
        /// Getters for properties.
        /// </summary>
        protected readonly EventPropertyGetter[] propertyGetters;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="streamNum">the stream number that is indexed</param>
        /// <param name="eventType">types of events indexed</param>
        /// <param name="propertyNames">property names to use for indexing</param>
        /// <param name="unique"></param>
        /// <param name="optionalIndexName"></param>
        public PropertyIndexedEventTableFactory(int streamNum, EventType eventType, IList<string> propertyNames, bool unique, string optionalIndexName)
        {
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.unique = unique;
            this.optionalIndexName = optionalIndexName;

            // Init getters
            propertyGetters = new EventPropertyGetter[propertyNames.Count];
            for (int i = 0; i < propertyNames.Count; i++)
            {
                propertyGetters[i] = eventType.GetGetter(propertyNames[i]);
            }
        }

        public virtual EventTable[] MakeEventTables(EventTableFactoryTableIdent tableIdent, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventTableOrganization organization = Organization;
            if (unique)
            {
                return new EventTable[] { new PropertyIndexedEventTableUnique(propertyGetters, organization) };
            }
            else
            {
                return new EventTable[] { new PropertyIndexedEventTableUnadorned(propertyGetters, organization) };
            }
        }

        public Type EventTableType
        {
            get
            {
                return unique ? typeof(PropertyIndexedEventTableUnique) : typeof(PropertyIndexedEventTableUnadorned);
            }
        }

        public string ToQueryPlan()
        {
            return string.Format("{0}{1} streamNum={2} propertyNames={3}",
                GetType().Name, (unique ? " unique" : " non-unique"), streamNum, propertyNames.Render());
        }

        public int StreamNum
        {
            get { return streamNum; }
        }

        public IList<string> PropertyNames
        {
            get { return propertyNames; }
        }

        public bool IsUnique
        {
            get { return unique; }
        }

        public string OptionalIndexName
        {
            get { return optionalIndexName; }
        }

        public EventPropertyGetter[] PropertyGetters
        {
            get { return propertyGetters; }
        }

        protected virtual EventTableOrganization Organization
        {
            get
            {
                return new EventTableOrganization(
                    optionalIndexName, unique, false,
                    streamNum, propertyNames, EventTableOrganizationType.HASH);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
