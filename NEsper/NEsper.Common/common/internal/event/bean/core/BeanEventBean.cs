///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Wrapper for regular objects the represent events.
    ///     Allows access to event properties, which is done through the getter supplied by the event type.
    ///     <seealso cref="EventType" /> instances containing type information are obtained from
    ///     <seealso cref="BeanEventTypeFactory" />.
    ///     Two BeanEventBean instances are equal if they have the same event type and refer to the same instance of event
    ///     object.
    ///     Clients that need to compute equality between beans wrapped by this class need to obtain the underlying
    ///     object.
    /// </summary>
    public class BeanEventBean : EventBeanSPI
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="theEvent">is the event object</param>
        /// <param name="eventType">is the schema information for the event object.</param>
        public BeanEventBean(
            object theEvent,
            EventType eventType)
        {
            EventType = eventType;
            Underlying = theEvent;
        }

        public object this[string property] => Get(property);

        public object UnderlyingSpi { get; set; }

        public object Underlying {
            get => UnderlyingSpi;
            set => UnderlyingSpi = value;
        }

        public EventType EventType { get; }

        public object Get(string property)
        {
            var getter = EventType.GetGetter(property);
            if (getter == null) {
                throw new PropertyAccessException(
                    "Property named '" + property + "' is not a valid property name for this type");
            }

            return getter.Get(this);
        }

        public object GetFragment(string propertyExpression)
        {
            var getter = EventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }

        public override string ToString()
        {
            return "BeanEventBean" +
                   " eventType=" + EventType +
                   " bean=" + Underlying;
        }
    }
} // end of namespace