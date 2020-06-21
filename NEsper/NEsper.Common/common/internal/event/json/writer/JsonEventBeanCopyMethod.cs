///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;

namespace com.espertech.esper.common.@internal.@event.json.writer
{
	/// <summary>
	///     Copy method for Json-underlying events.
	/// </summary>
	public class JsonEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly JsonEventType eventType;

        public JsonEventBeanCopyMethod(
            JsonEventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.eventType = eventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var source = theEvent.Underlying;
            var copy = eventType.DelegateFactory.Copy(source);
            return eventBeanTypedEventFactory.AdapterForTypedJson(copy, eventType);
        }
    }
} // end of namespace