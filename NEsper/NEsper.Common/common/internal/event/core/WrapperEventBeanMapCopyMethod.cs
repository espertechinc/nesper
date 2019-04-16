///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Copy method for wrapper events.
    /// </summary>
    public class WrapperEventBeanMapCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly WrapperEventType wrapperEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="wrapperEventType">wrapper type</param>
        /// <param name="eventAdapterService">event adapter</param>
        public WrapperEventBeanMapCopyMethod(
            WrapperEventType wrapperEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            this.wrapperEventType = wrapperEventType;
            this.eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var decorated = (DecoratingEventBean) theEvent;
            var decoratedUnderlying = decorated.UnderlyingEvent;
            IDictionary<string, object> copiedMap = new Dictionary<string, object>(decorated.DecoratingProperties);
            return eventAdapterService.AdapterForTypedWrapper(decoratedUnderlying, copiedMap, wrapperEventType);
        }
    }
} // end of namespace