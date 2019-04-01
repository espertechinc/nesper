///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.core;

namespace com.espertech.esper.common.@internal.@event.path
{
	public interface EventTypeResolver
	{
	    EventType Resolve(EventTypeMetadata metadata);

	    BeanEventType ResolvePrivateBean(Type clazz);
	}

    public class EventTypeResolverConstants
    {
        public const string RESOLVE_METHOD = "resolve";
        public const string RESOLVE_PRIVATE_BEAN_METHOD = "resolvePrivateBean";
    }
} // end of namespace