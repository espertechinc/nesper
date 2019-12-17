///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.hash
{
    public class PropertyHashedEventTableFactory : EventTableFactory
    {
        internal readonly string OptionalIndexName;
        internal readonly EventPropertyValueGetter PropertyGetter;
        internal readonly string[] PropertyNames;
        internal readonly int StreamNum;
        internal readonly bool Unique;

        public PropertyHashedEventTableFactory(
            int streamNum,
            string[] propertyNames,
            bool unique,
            string optionalIndexName,
            EventPropertyValueGetter propertyGetter)
        {
            StreamNum = streamNum;
            PropertyNames = propertyNames;
            Unique = unique;
            OptionalIndexName = optionalIndexName;
            PropertyGetter = propertyGetter;

            if (propertyGetter == null) {
                throw new ArgumentException("Property-getter is null");
            }
        }

        public EventTableOrganization Organization => new EventTableOrganization(
            OptionalIndexName,
            Unique,
            false,
            StreamNum,
            PropertyNames,
            EventTableOrganizationType.HASH);

        public EventTable[] MakeEventTables(
            AgentInstanceContext agentInstanceContext,
            int? subqueryNumber)
        {
            if (Unique) {
                return new EventTable[] {new PropertyHashedEventTableUnique(this)};
            }

            return new EventTable[] {new PropertyHashedEventTableUnadorned(this)};
        }

        public string ToQueryPlan()
        {
            return GetType().Name +
                   (Unique ? " unique" : " non-unique") +
                   " streamNum=" +
                   StreamNum +
                   " propertyNames=" +
                   Arrays.AsList(PropertyNames);
        }

        public Type EventTableClass =>
            Unique ? typeof(PropertyHashedEventTableUnique) : typeof(PropertyHashedEventTableUnadorned);
    }
} // end of namespace