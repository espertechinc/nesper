///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;

using PropertyInfo = System.Reflection.PropertyInfo;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    public class ReflectionPropMethodGetterFactory : EventPropertyGetterSPIFactory
    {
        private readonly MethodInfo _method;
        private readonly PropertyInfo _property;

        public ReflectionPropMethodGetterFactory(MethodInfo method)
        {
            _method = method;
            _property = null;
        }

        public ReflectionPropMethodGetterFactory(PropertyInfo property)
        {
            _method = null;
            _property = property;
        }

        public EventPropertyGetterSPI Make(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            if (_method != null) {
                return new ReflectionPropMethodGetter(_method, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            if (_property != null) {
                return new ReflectionPropMethodGetter(_property, eventBeanTypedEventFactory, beanEventTypeFactory);
            }

            throw new IllegalStateException("Both _method && _property are null");
        }
    }
} // end of namespace