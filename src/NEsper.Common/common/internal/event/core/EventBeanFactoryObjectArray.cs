///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanFactoryObjectArray : EventBeanFactory
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EventType _type;

        public EventBeanFactoryObjectArray(
            EventType type,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this._type = type;
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public Type UnderlyingType => typeof(object[]);

        public EventBean Wrap(object underlying)
        {
            return _eventBeanTypedEventFactory.AdapterForTypedObjectArray((object[]) underlying, _type);
        }
    }
} // end of namespace