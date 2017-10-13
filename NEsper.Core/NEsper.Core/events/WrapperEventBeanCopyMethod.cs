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

namespace com.espertech.esper.events
{
    /// <summary>Copy method for wrapper events. </summary>
    public class WrapperEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly WrapperEventType _wrapperEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventBeanCopyMethod _underlyingCopyMethod;
    
        /// <summary>Ctor. </summary>
        /// <param name="wrapperEventType">wrapper type</param>
        /// <param name="eventAdapterService">event adapter creation</param>
        /// <param name="underlyingCopyMethod">copy method for the underlying event</param>
        public WrapperEventBeanCopyMethod(WrapperEventType wrapperEventType, EventAdapterService eventAdapterService, EventBeanCopyMethod underlyingCopyMethod)
        {
            _wrapperEventType = wrapperEventType;
            _eventAdapterService = eventAdapterService;
            _underlyingCopyMethod = underlyingCopyMethod;
        }
    
        public EventBean Copy(EventBean theEvent)
        {
            DecoratingEventBean decorated = (DecoratingEventBean) theEvent;
            EventBean decoratedUnderlying = decorated.UnderlyingEvent;
            EventBean copiedUnderlying = _underlyingCopyMethod.Copy(decoratedUnderlying);
            if (copiedUnderlying == null)
            {
                return null;
            }
            IDictionary<String, Object> copiedMap = new Dictionary<String, Object>(decorated.DecoratingProperties);
            return _eventAdapterService.AdapterForTypedWrapper(copiedUnderlying, copiedMap, _wrapperEventType);
        }
    }
}
