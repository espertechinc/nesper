///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.service
{
	public class BeanEventTypeFactoryDisallow : BeanEventTypeFactory {
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

	    public BeanEventTypeFactoryDisallow(EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	    }

	    public BeanEventType GetCreateBeanType(Type clazz) {
	        throw new EPException("Bean type creation not supported");
	    }

	    public EventBeanTypedEventFactory EventBeanTypedEventFactory {
	        get => eventBeanTypedEventFactory;
	    }

	    public EventTypeFactory EventTypeFactory {
	        get { throw new EPException("Event type creation not supported"); }
	    }
	}
} // end of namespace