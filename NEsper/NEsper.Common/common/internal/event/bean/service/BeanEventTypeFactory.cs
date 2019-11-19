///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;

namespace com.espertech.esper.common.@internal.@event.bean.service
{
    /// <summary>
    ///     Interface for a factory for obtaining <seealso cref="BeanEventType" /> instances.
    /// </summary>
    public interface BeanEventTypeFactory
    {
        EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        EventTypeFactory EventTypeFactory { get; }
        BeanEventType GetCreateBeanType(Type clazz);
    }
} // end of namespace