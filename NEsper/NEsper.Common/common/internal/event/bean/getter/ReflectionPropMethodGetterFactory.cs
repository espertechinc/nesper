///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    public class ReflectionPropMethodGetterFactory : EventPropertyGetterSPIFactory
    {
        private readonly MethodInfo method;

        public ReflectionPropMethodGetterFactory(MethodInfo method)
        {
            this.method = method;
        }

        public EventPropertyGetterSPI Make(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropMethodGetter(method, eventBeanTypedEventFactory, beanEventTypeFactory);
        }
    }
} // end of namespace