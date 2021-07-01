///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanFactoryBeanWrapped : EventBeanFactory
    {
        private readonly EventType beanEventType;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType wrapperEventType;

        public EventBeanFactoryBeanWrapped(
            EventType beanEventType,
            EventType wrapperEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.beanEventType = beanEventType;
            this.wrapperEventType = wrapperEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public Type UnderlyingType => beanEventType.UnderlyingType;

        public EventBean Wrap(object underlying)
        {
            var bean = eventBeanTypedEventFactory.AdapterForTypedObject(underlying, beanEventType);
            return eventBeanTypedEventFactory.AdapterForTypedWrapper(
                bean,
                Collections.GetEmptyMap<string, object>(),
                wrapperEventType);
        }
    }
} // end of namespace