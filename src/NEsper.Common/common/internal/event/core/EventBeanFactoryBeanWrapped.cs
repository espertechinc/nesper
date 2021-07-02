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
        private readonly EventType _beanEventType;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EventType _wrapperEventType;

        public EventBeanFactoryBeanWrapped(
            EventType beanEventType,
            EventType wrapperEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this._beanEventType = beanEventType;
            this._wrapperEventType = wrapperEventType;
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public Type UnderlyingType => _beanEventType.UnderlyingType;

        public EventBean Wrap(object underlying)
        {
            var bean = _eventBeanTypedEventFactory.AdapterForTypedObject(underlying, _beanEventType);
            return _eventBeanTypedEventFactory.AdapterForTypedWrapper(
                bean,
                Collections.GetEmptyMap<string, object>(),
                _wrapperEventType);
        }
    }
} // end of namespace