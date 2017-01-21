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
    /// <summary>
    /// An event that is carries multiple representations of event properties: A
    /// synthetic representation that is designed for delivery as <seealso cref="EventBean"/> to
    /// client <seealso cref="com.espertech.esper.client.UpdateListener"/> code, and a
    /// natural representation as a bunch of Object-type properties for fast delivery to
    /// client subscriber objects via method call.
    /// </summary>
    public class NaturalEventBean : EventBean, DecoratingEventBean
    {
        private readonly EventType _eventBeanType;
        private readonly Object[] _natural;
        private readonly EventBean _optionalSynthetic;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventBeanType">the event type of the synthetic event</param>
        /// <param name="natural">the properties of the event</param>
        /// <param name="optionalSynthetic">the event bean that is the synthetic event, or null if no synthetic is packed in</param>
        public NaturalEventBean(EventType eventBeanType, Object[] natural, EventBean optionalSynthetic) {
            _eventBeanType = eventBeanType;
            _natural = natural;
            _optionalSynthetic = optionalSynthetic;
        }

        public EventType EventType
        {
            get { return _eventBeanType; }
        }

        #region Implementation of EventBean

        public object this[string property]
        {
            get { return Get(property); }
        }

        #endregion

        public Object Get(String property)
        {
            if (_optionalSynthetic != null)
            {
                return _optionalSynthetic.Get(property);
            }
            throw new PropertyAccessException("Property access not allowed for natural events without the synthetic event present");
        }

        public object Underlying
        {
            get
            {
                return _optionalSynthetic != null 
                    ? _optionalSynthetic.Underlying
                    : _natural;
            }
        }

        public EventBean UnderlyingEvent
        {
            get { return ((DecoratingEventBean) _optionalSynthetic).UnderlyingEvent; }
        }

        public IDictionary<string, object> DecoratingProperties
        {
            get { return ((DecoratingEventBean) _optionalSynthetic).DecoratingProperties; }
        }

        /// <summary>
        /// Returns the column object result representation.
        /// </summary>
        /// <returns>
        /// select column values
        /// </returns>
        public object[] Natural
        {
            get { return _natural; }
        }

        /// <summary>
        /// Returns the synthetic event that can be attached.
        /// </summary>
        /// <returns>
        /// synthetic if attached, or null if none attached
        /// </returns>
        public EventBean OptionalSynthetic
        {
            get { return _optionalSynthetic; }
        }

        public Object GetFragment(String propertyExpression)
        {
            if (_optionalSynthetic != null)
            {
                return _optionalSynthetic.GetFragment(propertyExpression);
            }
            throw new PropertyAccessException("Property access not allowed for natural events without the synthetic event present");
        }
    }
}
