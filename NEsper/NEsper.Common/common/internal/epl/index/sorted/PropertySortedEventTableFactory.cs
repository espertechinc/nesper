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
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.index.sorted
{
    /// <summary>
    ///     Index that organizes events by the event property values into a single TreeMap sortable non-nested index
    ///     with Object keys that store the property values.
    /// </summary>
    public class PropertySortedEventTableFactory : EventTableFactory
    {
        protected internal readonly EventPropertyValueGetter propertyGetter;
        protected internal readonly string propertyName;
        protected internal readonly int streamNum;
        protected internal readonly Type valueType;

        public PropertySortedEventTableFactory(
            int streamNum,
            string propertyName,
            EventPropertyValueGetter propertyGetter,
            Type valueType)
        {
            this.streamNum = streamNum;
            this.propertyName = propertyName;
            this.propertyGetter = propertyGetter;
            this.valueType = valueType;
        }

        public int StreamNum => streamNum;

        public string PropertyName => propertyName;

        public Type ValueType => valueType;

        public EventTableOrganization Organization => new EventTableOrganization(
            null, false, false, streamNum, new[] {propertyName}, EventTableOrganizationType.BTREE);

        public EventTable[] MakeEventTables(
            AgentInstanceContext agentInstanceContext,
            int? subqueryNumber)
        {
            return new EventTable[] {new PropertySortedEventTableImpl(this)};
        }

        public Type EventTableClass => typeof(PropertySortedEventTable);

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() +
                   " streamNum=" + streamNum +
                   " propertyName=" + propertyName;
        }
    }
} // end of namespace