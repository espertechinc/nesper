///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events
{
    public class EventBeanFactoryBeanWrapped : EventBeanFactory
    {
        private readonly EventType _beanEventType;
        private readonly EventType _wrapperEventType;
        private readonly EventAdapterService _eventAdapterService;

        public EventBeanFactoryBeanWrapped(EventType beanEventType, EventType wrapperEventType, EventAdapterService eventAdapterService)
        {
            _beanEventType = beanEventType;
            _wrapperEventType = wrapperEventType;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Wrap(Object underlying)
        {
            EventBean bean = _eventAdapterService.AdapterForTypedObject(underlying, _beanEventType);
            return _eventAdapterService.AdapterForTypedWrapper(bean, new EmptyDictionary<string, object>(), _wrapperEventType);
        }
    }
}
