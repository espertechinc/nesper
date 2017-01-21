///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    public class EventBeanFactoryBean : EventBeanFactory
    {
        private readonly EventType _type;
        private readonly EventAdapterService _eventAdapterService;

        public EventBeanFactoryBean(EventType type, EventAdapterService eventAdapterService)
        {
            _type = type;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Wrap(Object underlying)
        {
            return _eventAdapterService.AdapterForTypedObject(underlying, _type);
        }
    }
}
