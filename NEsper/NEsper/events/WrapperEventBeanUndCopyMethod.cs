///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>Copy method for underlying events. </summary>
    public class WrapperEventBeanUndCopyMethod : EventBeanCopyMethod
    {
        private readonly WrapperEventType _wrapperEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventBeanCopyMethod _underlyingCopyMethod;
    
        /// <summary>Ctor. </summary>
        /// <param name="wrapperEventType">wrapper type</param>
        /// <param name="eventAdapterService">for creating events</param>
        /// <param name="underlyingCopyMethod">for copying the underlying event</param>
        public WrapperEventBeanUndCopyMethod(WrapperEventType wrapperEventType, EventAdapterService eventAdapterService, EventBeanCopyMethod underlyingCopyMethod)
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
            return _eventAdapterService.AdapterForTypedWrapper(copiedUnderlying, decorated.DecoratingProperties, _wrapperEventType);
        }
    }
}
