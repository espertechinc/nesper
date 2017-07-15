///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Wrapper for vanilla objects Java objects the represent events.
    /// Allows access to event properties, which is done through the getter supplied by the event type.
    /// <seealso cref="client.EventType" /> instances containing type information are obtained from <seealso cref="BeanEventTypeFactory" />.
    /// Two BeanEventBean instances are equal if they have the same event type and refer to the same instance of event object.
    /// Clients that need to compute equality between objects wrapped by this class need to obtain the underlying object.
    /// </summary>
    public class BeanEventBean : EventBeanSPI
    {
        private readonly EventType _eventType;
        private object _theEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="theEvent">is the event object</param>
        /// <param name="eventType">is the schema information for the event object.</param>
        public BeanEventBean(Object theEvent, EventType eventType)
        {
            this._eventType = eventType;
            this._theEvent = theEvent;
        }

        public object Underlying
        {
            get { return _theEvent; }
            set { _theEvent = value; }
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public Object Get(string property)
        {
            EventPropertyGetter getter = _eventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException(
                    "Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public override String ToString()
        {
            return "BeanEventBean" +
                   " eventType=" + _eventType +
                   " bean=" + _theEvent;
        }

        public Object GetFragment(string propertyExpression)
        {
            EventPropertyGetter getter = _eventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }
            return getter.GetFragment(this);
        }
    }
} // end of namespace
