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
    public class ReflectionPropFieldGetterFactory : EventPropertyGetterSPIFactory
    {
        private readonly FieldInfo field;

        public ReflectionPropFieldGetterFactory(FieldInfo field)
        {
            this.field = field;
        }

        public EventPropertyGetterSPI Make(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropFieldGetter(field, eventBeanTypedEventFactory, beanEventTypeFactory);
        }
    }
} // end of namespace