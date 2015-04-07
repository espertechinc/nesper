///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    public class EventBeanManufacturerCtor : EventBeanManufacturer
    {
        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly FastConstructor _fastConstructor;

        public EventBeanManufacturerCtor(
            FastConstructor fastConstructor,
            BeanEventType beanEventType,
            EventAdapterService eventAdapterService)
        {
            _fastConstructor = fastConstructor;
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
                properties, _fastConstructor, _beanEventType.UnderlyingType);
        }
    }
} // end of namespace