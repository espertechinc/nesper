///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Wrapper for events represented by a Map of key-value pairs that are the event properties.
    ///     MapEventBean instances are equal if they have the same <seealso cref="EventType" /> and all property names
    ///     and values are reference-equal.
    /// </summary>
    public class MapEventBean : EventBeanSPI,
        MappedEventBean
    {
        /// <summary>
        ///     Constructor for initialization with existing values.
        ///     Makes a shallow copy of the supplied values to not be surprised by changing property values.
        /// </summary>
        /// <param name="properties">are the event property values</param>
        /// <param name="eventType">is the type of the event, i.e. describes the map entries</param>
        public MapEventBean(
            IDictionary<string, object> properties,
            EventType eventType)
        {
            Properties = properties;
            EventType = eventType;
        }

        /// <summary>
        ///     Constructor for the mutable functions, e.g. only the type of values is known but not the actual values.
        /// </summary>
        /// <param name="eventType">is the type of the event, i.e. describes the map entries</param>
        public MapEventBean(EventType eventType)
        {
            Properties = new Dictionary<string, object>();
            EventType = eventType;
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

        public object Underlying {
            get => Properties;
            set => Properties = (IDictionary<string, object>)value;
        }

        public object UnderlyingSpi {
            get => Underlying;
            set => Underlying = value;
        }

        public object GetFragment(string propertyExpression)
        {
            var getter = EventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }

        public object this[string property] => Get(property);

        /// <summary>
        ///     Returns the properties.
        /// </summary>
        /// <value>properties</value>
        public IDictionary<string, object> Properties { get; private set; }

        public override string ToString()
        {
            return "MapEventBean " +
                   "eventType=" +
                   EventType;
        }
    }
} // end of namespace