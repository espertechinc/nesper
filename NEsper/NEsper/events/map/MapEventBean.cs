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

namespace com.espertech.esper.events.map
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Wrapper for events represented by a Map of key-value pairs that are the event
    /// properties. MapEventBean instances are equal if they have the same <seealso
    /// cref="client.EventType"/> and all property names and values are reference-equal.
    /// </summary>
    public class MapEventBean
        : EventBeanSPI
        , MappedEventBean
    {
        private readonly EventType _eventType;
        private DataMap _properties;
    
        /// <summary>
        /// Constructor for initialization with existing values. Makes a shallow copy of the
        /// supplied values to not be surprised by changing property values.
        /// </summary>
        /// <param name="properties">are the event property values</param>
        /// <param name="eventType">is the type of the event, i.e. describes the map entries</param>
        public MapEventBean(DataMap properties, EventType eventType)
        {
            _eventType = eventType;
            _properties = properties;
        }

        /// <summary>
        /// Constructor for the mutable functions, e.g. only the type of values is known but
        /// not the actual values.
        /// </summary>
        /// <param name="eventType">is the type of the event, i.e. describes the map entries</param>
        public MapEventBean(MapEventType eventType)
        {
            _properties = new Dictionary<string, object>();
            _eventType = eventType;
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>
        /// Returns the properties.
        /// </summary>
        /// <returns>
        /// properties
        /// </returns>
        public DataMap Properties
        {
            get { return _properties; }
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public Object Get(String property)
        {
            EventPropertyGetter getter = _eventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }

        public object Underlying
        {
            get { return _properties; }
            set { _properties = value as DataMap; }
        }

        public override String ToString()
        {
            return "MapEventBean " +
                    "eventType=" + _eventType;
        }
    
        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = _eventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + propertyExpression + "' is not a valid property name for this type");
            }
            return getter.GetFragment(this);
        }
    }
}
