///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for ObjectArray-underlying events.
    /// </summary>
    public class EventBeanManufacturerObjectArray : EventBeanManufacturer
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly ObjectArrayEventType eventType;
        private readonly int[] indexPerWritable;
        private readonly bool oneToOne;

        public EventBeanManufacturerObjectArray(
            ObjectArrayEventType eventType,
            EventBeanTypedEventFactory eventAdapterService,
            int[] indexPerWritable,
            bool oneToOne)
        {
            this.eventType = eventType;
            this.eventAdapterService = eventAdapterService;
            this.indexPerWritable = indexPerWritable;
            this.oneToOne = oneToOne;
        }

        public EventBean Make(object[] properties)
        {
            var cols = MakeUnderlying(properties);
            return eventAdapterService.AdapterForTypedObjectArray(cols, eventType);
        }

        object EventBeanManufacturer.MakeUnderlying(object[] properties)
        {
            return MakeUnderlying(properties);
        }

        public object[] MakeUnderlying(object[] properties)
        {
            if (oneToOne) {
                return properties;
            }

            var cols = new object[eventType.PropertyNames.Length];
            for (var i = 0; i < properties.Length; i++) {
                var indexToWrite = indexPerWritable[i];
                cols[indexToWrite] = properties[i];
            }

            return cols;
        }
    }
} // end of namespace