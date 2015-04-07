///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Copy method for bean events utilizing serializable.
    /// </summary>
    public class BeanEventBeanSerializableCopyMethod : EventBeanCopyMethod
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _eventAdapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="beanEventType">event type</param>
        /// <param name="eventAdapterService">for creating the event object</param>
        public BeanEventBeanSerializableCopyMethod(BeanEventType beanEventType, EventAdapterService eventAdapterService)
        {
            _beanEventType = beanEventType;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            Object underlying = theEvent.Underlying;
            Object copied;
            try
            {
                copied = SerializableObjectCopier.Copy(underlying);
            }
            catch (IOException e)
            {
                Log.Error("IOException copying event object for Update: " + e.Message, e);
                return null;
            }
            catch (TypeLoadException e)
            {
                Log.Error("Exception copying event object for Update: " + e.Message, e);
                return null;
            }

            return _eventAdapterService.AdapterForTypedObject(copied, _beanEventType);
        }
    }
}
