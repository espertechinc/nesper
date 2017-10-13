///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    public class EventBeanManufacturerCtor : EventBeanManufacturer
    {
        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly ConstructorInfo _constructor;

        public EventBeanManufacturerCtor(
            ConstructorInfo constructorInfo,
            BeanEventType beanEventType,
            EventAdapterService eventAdapterService)
        {
            _constructor = constructorInfo;
            _beanEventType = beanEventType;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Make(object[] properties)
        {
            object instance = MakeUnderlying(properties);
            return _eventAdapterService.AdapterForTypedObject(instance, _beanEventType);
        }

        public object MakeUnderlying(object[] properties)
        {
            return InstanceManufacturerFastCtor.MakeUnderlyingFromFastCtor(
                properties, _constructor, _beanEventType.UnderlyingType);
        }
    }
} // end of namespace