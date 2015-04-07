///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    public class EventBeanFactoryObjectArray : EventBeanFactory
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _type;

        public EventBeanFactoryObjectArray(EventType type, EventAdapterService eventAdapterService)
        {
            _type = type;
            _eventAdapterService = eventAdapterService;
        }

        public Type UnderlyingType
        {
            get { return typeof (object[]); }
        }

        #region EventBeanFactory Members

        public EventBean Wrap(Object underlying)
        {
            return _eventAdapterService.AdapterForTypedObjectArray((Object[]) underlying, _type);
        }

        #endregion
    }
}