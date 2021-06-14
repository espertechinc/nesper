///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

using PropertyInfo = System.Reflection.PropertyInfo;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    public class ReflectionPropPropertyGetterFactory : EventPropertyGetterSPIFactory
    {
        private readonly PropertyInfo _property;

        public ReflectionPropPropertyGetterFactory(PropertyInfo property)
        {
            _property = property;
        }

        public EventPropertyGetterSPI Make(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropPropertyGetter(_property, eventBeanTypedEventFactory, beanEventTypeFactory);
        }
    }
} // end of namespace