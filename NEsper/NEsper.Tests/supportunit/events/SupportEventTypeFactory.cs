///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.support;
using com.espertech.esper.events;
using com.espertech.esper.supportunit.util;

namespace com.espertech.esper.supportunit.events
{
    public class SupportEventTypeFactory
    {
        public static EventType CreateBeanType(Type clazz, String name)
        {
            return SupportContainer.Instance.Resolve<EventAdapterService>().AddBeanType(name, clazz, false, false, false);
        }
    
        public static EventType CreateBeanType(Type clazz)
        {
            return SupportContainer.Instance.Resolve<EventAdapterService>().AddBeanType(clazz.FullName, clazz, false, false, false);
        }
    
        public static EventType CreateMapType(IDictionary<String,Object> map)
        {
            return SupportContainer.Instance.Resolve<EventAdapterService>().CreateAnonymousMapType("test", map, true);
        }
    }
}
