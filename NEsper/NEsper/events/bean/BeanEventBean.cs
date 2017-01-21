///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Wrapper for PONO objects that represent events.
    /// Allows access to event properties, which is done through the getter supplied by the
    /// event type. <seealso cref="client.EventType"/> instances containing type information are
    /// obtained from <seealso cref="BeanEventTypeFactory"/>. Two BeanEventBean instances
    /// are equal if they have the same event type and refer to the same instance of
    /// event object. Clients that need to compute equality between object wrapped by
    /// this class need to obtain the underlying object.
    /// </summary>
    public sealed class BeanEventBean : EventBeanSPI
    {
        private object _underlying;
        private EventType _eventType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="theEvent">is the event object</param>
        /// <param name="eventType">is the schema information for the event object.</param>
        public BeanEventBean(Object theEvent, EventType eventType)
        {
            _eventType = eventType;
            _underlying = theEvent;
        }

        public object Underlying
        {
            get { return _underlying; }
            set { _underlying = value; }
        }

        public EventType EventType
        {
            get { return _eventType; }
            private set { _eventType = value; }
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public Object Get(String property)
        {
            EventPropertyGetter getter = EventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }
    
        public override String ToString()
        {
            return "BeanEventBean" +
                   " eventType=" + EventType +
                   " eventObject=" + Underlying;
        }
    
        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = EventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + propertyExpression + "' is not a valid property name for this type");
            }
            return getter.GetFragment(this);
        }
    }
}
