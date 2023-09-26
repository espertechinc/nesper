///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.json.core;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanFactoryJson : EventBeanFactory
    {
        private readonly EventBeanTypedEventFactory _factory;
        private readonly JsonEventType _type;

        public EventBeanFactoryJson(
            JsonEventType type,
            EventBeanTypedEventFactory factory)
        {
            _type = type;
            _factory = factory;
        }

        public Type UnderlyingType => _type.UnderlyingType;

        public EventBean Wrap(object underlying)
        {
            var und = _type.Parse((string)underlying);
            return _factory.AdapterForTypedJson(und, _type);
        }
    }
} // end of namespace