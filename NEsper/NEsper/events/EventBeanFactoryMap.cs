///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    using DataMap = System.Collections.Generic.IDictionary<string, object>;

    public class EventBeanFactoryMap : EventBeanFactory
    {
        private readonly EventType _type;
        private readonly EventAdapterService _eventAdapterService;

        public EventBeanFactoryMap(EventType type, EventAdapterService eventAdapterService)
        {
            _type = type;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Wrap(Object underlying)
        {
            return _eventAdapterService.AdapterForTypedMap((DataMap)underlying, _type);
        }
    }
}
