///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    public class EventBeanManufacturerCtor : EventBeanManufacturer
    {
        private readonly BeanEventType beanEventType;
        private readonly ConstructorInfo constructor;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

        public EventBeanManufacturerCtor(
            ConstructorInfo constructor,
            EventType beanEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.constructor = constructor;
            this.beanEventType = (BeanEventType)beanEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public EventBean Make(object[] properties)
        {
            var instance = MakeUnderlying(properties);
            return eventBeanTypedEventFactory.AdapterForTypedObject(instance, beanEventType);
        }

        public object MakeUnderlying(object[] properties)
        {
            return InstanceManufacturerFastCtor.MakeUnderlyingFromFastCtor(
                properties,
                constructor,
                beanEventType.UnderlyingType);
        }
    }
} // end of namespace