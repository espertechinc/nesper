///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly BeanEventType _beanEventType;
        private readonly ConstructorInfo _constructor;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;

        public EventBeanManufacturerCtor(
            ConstructorInfo constructor,
            EventType beanEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this._constructor = constructor;
            this._beanEventType = (BeanEventType) beanEventType;
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public EventBean Make(object[] properties)
        {
            var instance = MakeUnderlying(properties);
            return _eventBeanTypedEventFactory.AdapterForTypedObject(instance, _beanEventType);
        }

        public object MakeUnderlying(object[] properties)
        {
            return InstanceManufacturerFastCtor.MakeUnderlyingFromFastCtor(
                properties,
                _constructor,
                _beanEventType.UnderlyingType);
        }
    }
} // end of namespace