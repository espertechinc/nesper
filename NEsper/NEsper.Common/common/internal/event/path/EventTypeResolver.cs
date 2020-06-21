///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.runtime.@event;

namespace com.espertech.esper.common.@internal.@event.path
{
    public interface EventTypeResolver
    {
        EventTypeSPI Resolve(EventTypeMetadata metadata);

        BeanEventType ResolvePrivateBean(
            Type clazz,
            bool publicFields);
        
        EventSerdeFactory GetEventSerdeFactory();
    }

    public class EventTypeResolverConstants
    {
        public const string RESOLVE_METHOD = "Resolve";
        public const string RESOLVE_PRIVATE_BEAN_METHOD = "ResolvePrivateBean";
        public const string GETEVENTSERDEFACTORY = "GetEventSerdeFactory";
    }
} // end of namespace