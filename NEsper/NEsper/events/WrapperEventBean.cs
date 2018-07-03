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
using com.espertech.esper.collection;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Event bean that wraps another event bean adding additional properties.
    /// <para>
    /// This can be useful for classes for which the statement adds derived values retaining the original class.
    /// </para>
    /// <para>
    /// The event type of such events is always <see cref="WrapperEventType"/>. Additional properties are stored in a
    /// Map.
    /// </para>
    /// </summary>
    public class WrapperEventBean
        : EventBean
        , DecoratingEventBean
    {
        private readonly EventBean _theEvent;
        private readonly DataMap _map;
        private readonly EventType _eventType;

        /// <summary>Ctor.</summary>
        /// <param name="theEvent">is the wrapped event</param>
        /// <param name="properties">
        /// is zero or more property values that embellish the wrapped event
        /// </param>
        /// <param name="eventType">is the <see cref="WrapperEventType"/>.</param>
        public WrapperEventBean(EventBean theEvent, DataMap properties, EventType eventType)
        {
            _theEvent = theEvent;
            _map = properties;
            _eventType = eventType;
        }

        /// <summary>
        /// Returns the value of an event property.
        /// </summary>
        /// <value></value>
        /// <returns> the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public Object this[String property] => GetInternal(property);

        /// <summary>
        /// Returns the value of an event property.  This method is a proxy of the indexer.
        /// </summary>
        /// <param name="property">name of the property whose value is to be retrieved</param>
        /// <returns>
        /// the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public virtual Object Get(String property)
        {
            return GetInternal(property);
        }

        /// <summary>
        /// Internal getter implementation.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        /// <exception cref="PropertyAccessException">Property named '" + property + "' is not a valid property name for this type</exception>
        private object GetInternal(string property)
        {
            EventPropertyGetter getter = _eventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }

        /// <summary>
        /// Return the <see cref="EventType"/> instance that describes the set of properties available for this event.
        /// </summary>
        /// <value></value>
        /// <returns> event type
        /// </returns>
        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>
        /// Get the underlying data object to this event wrapper.
        /// </summary>
        /// <value></value>
        /// <returns> underlying data object, usually either a Map or a bean instance.
        /// </returns>
        public Object Underlying
        {
            get
            {
                // If wrapper is simply for the underlying with no additional properties, then return the underlying type
                if (_map.Count == 0)
                {
                    return _theEvent.Underlying;
                }
                else
                {
                    return new Pair<Object, DataMap>(_theEvent.Underlying, _map);
                }
            }
        }

        /// <summary>
        /// Returns the underlying map storing the additional properties, if any.
        /// </summary>
        /// <returns>event property IDictionary</returns>
        public DataMap UnderlyingMap
        {
            get { return _map; }
        }

        /// <summary>
        /// Returns decorating properties.
        /// </summary>
        /// <value></value>
        /// <returns>property name and values</returns>
        public DataMap DecoratingProperties
        {
            get { return _map; }
        }

        /// <summary>Returns the wrapped event.</summary>
        /// <returns>wrapped event</returns>
        public EventBean UnderlyingEvent
        {
            get { return _theEvent; }
        }


        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = _eventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }
            return getter.GetFragment(this);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return
                "WrapperEventBean " +
                "[event=" + _theEvent + "] " +
                "[properties=" + _map + "]";
        }
    }
} // End of namespace
