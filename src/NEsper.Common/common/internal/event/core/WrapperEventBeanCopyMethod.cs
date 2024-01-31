///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class WrapperEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory eventAdapterService;
        private readonly EventBeanCopyMethod underlyingCopyMethod;
        private readonly WrapperEventType wrapperEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="wrapperEventType">wrapper type</param>
        /// <param name="eventAdapterService">event adapter creation</param>
        /// <param name="underlyingCopyMethod">copy method for the underlying event</param>
        public WrapperEventBeanCopyMethod(
            WrapperEventType wrapperEventType,
            EventBeanTypedEventFactory eventAdapterService,
            EventBeanCopyMethod underlyingCopyMethod)
        {
            this.wrapperEventType = wrapperEventType;
            this.eventAdapterService = eventAdapterService;
            this.underlyingCopyMethod = underlyingCopyMethod;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var decorated = (DecoratingEventBean)theEvent;
            var decoratedUnderlying = decorated.UnderlyingEvent;
            var copiedUnderlying = underlyingCopyMethod.Copy(decoratedUnderlying);
            if (copiedUnderlying == null) {
                return null;
            }

            IDictionary<string, object> copiedMap = new Dictionary<string, object>(decorated.DecoratingProperties);
            return eventAdapterService.AdapterForTypedWrapper(copiedUnderlying, copiedMap, wrapperEventType);
        }
    }
} // end of namespace