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
    public class WrapperEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly EventBeanCopyMethod _underlyingCopyMethod;
        private readonly WrapperEventType _wrapperEventType;

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
            this._wrapperEventType = wrapperEventType;
            this._eventAdapterService = eventAdapterService;
            this._underlyingCopyMethod = underlyingCopyMethod;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var decorated = (DecoratingEventBean) theEvent;
            var decoratedUnderlying = decorated.UnderlyingEvent;
            var copiedUnderlying = _underlyingCopyMethod.Copy(decoratedUnderlying);
            if (copiedUnderlying == null) {
                return null;
            }

            IDictionary<string, object> copiedMap = new Dictionary<string, object>(decorated.DecoratingProperties);
            return _eventAdapterService.AdapterForTypedWrapper(copiedUnderlying, copiedMap, _wrapperEventType);
        }
    }
} // end of namespace