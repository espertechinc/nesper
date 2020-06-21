///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.inkeyword
{
    public class PropertyHashedArrayFactoryFactory : EventTableFactoryFactory
    {
        private readonly int streamNum;
        private readonly string[] propertyNames;
        private readonly Type[] propertyTypes;
        private readonly DataInputOutputSerde<Object>[] propertySerdes;
        private readonly bool unique;
        private readonly EventPropertyValueGetter[] propertyGetters;
        private readonly bool isFireAndForget;

        public PropertyHashedArrayFactoryFactory(
            int streamNum,
            string[] propertyNames,
            Type[] propertyTypes,
            DataInputOutputSerde<object>[] propertySerdes,
            bool unique,
            EventPropertyValueGetter[] propertyGetters,
            bool isFireAndForget)
        {
            this.streamNum = streamNum;
            this.propertyNames = propertyNames;
            this.propertyTypes = propertyTypes;
            this.propertySerdes = propertySerdes;
            this.unique = unique;
            this.propertyGetters = propertyGetters;
            this.isFireAndForget = isFireAndForget;
        }

        public EventTableFactory Create(EventType eventType, EventTableFactoryFactoryContext eventTableFactoryContext)
        {
            return eventTableFactoryContext.EventTableIndexService.CreateInArray(
                streamNum,
                eventType,
                propertyNames,
                propertyTypes,
                propertySerdes,
                unique,
                propertyGetters,
                isFireAndForget,
                eventTableFactoryContext);
        }
    }
} // end of namespace