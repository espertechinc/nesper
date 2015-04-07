///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Copies an event for modification.
    /// </summary>
    public class BeanEventBeanConfiguredCopyMethod : EventBeanCopyMethod
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _eventAdapterService;
        private readonly FastMethod _copyMethod;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="beanEventType">type of bean to copy</param>
        /// <param name="eventAdapterService">for creating events</param>
        /// <param name="copyMethod">method to copy the event</param>
        public BeanEventBeanConfiguredCopyMethod(BeanEventType beanEventType, EventAdapterService eventAdapterService, FastMethod copyMethod)
        {
            _beanEventType = beanEventType;
            _eventAdapterService = eventAdapterService;
            _copyMethod = copyMethod;
        }

        public EventBean Copy(EventBean theEvent)
        {
            Object underlying = theEvent.Underlying;
            Object copied;
            try
            {
                copied = _copyMethod.Invoke(underlying, null);
            }
            catch (TargetInvocationException e)
            {
                Log.Error("TargetInvocationException copying event object for Update: " + e.Message, e);
                return null;
            }
            catch (Exception e)
            {
                Log.Error("RuntimeException copying event object for Update: " + e.Message, e);
                return null;
            }

            return _eventAdapterService.AdapterForTypedObject(copied, _beanEventType);
        }
    }
}
