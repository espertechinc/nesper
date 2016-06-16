///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.support.events
{
    public class SupportEventTypeFactory
    {
        public static EventType CreateBeanType(Type clazz, String name)
        {
            return SupportEventAdapterService.Service.AddBeanType(name, clazz, false, false, false);
        }
    
        public static EventType CreateBeanType(Type clazz)
        {
            return SupportEventAdapterService.Service.AddBeanType(clazz.FullName, clazz, false, false, false);
        }
    
        public static EventType CreateMapType(IDictionary<String,Object> map)
        {
            return SupportEventAdapterService.Service.CreateAnonymousMapType("test", map, true);
        }
    }
}
