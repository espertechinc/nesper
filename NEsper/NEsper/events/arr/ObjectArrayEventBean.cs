///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    public class ObjectArrayEventBean
        : EventBeanSPI
        , ObjectArrayBackedEventBean
    {
        private Object[] _propertyValues;
        private EventType _eventType;

        public ObjectArrayEventBean(Object[] propertyValues, EventType eventType)
        {
            _propertyValues = propertyValues;
            _eventType = eventType;
        }

        public EventType EventType
        {
            get { return _eventType; }
            set { _eventType = value; }
        }

        public object[] Properties
        {
            get { return _propertyValues; }
        }

        public object[] PropertyValues
        {
            set { _propertyValues = value; }
        }

        /// <summary>
        /// Gets the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public Object Get(String property)
        {
            var getter = _eventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }

        /// <summary>
        /// Returns the value of an event property for the given property name or property expression.
        /// <para/> Returns null if the property value is null. Throws an exception if the expression is not valid against the event type.
        /// <para/> The method takes a property name or property expression as a parameter. Property expressions may include indexed properties
        /// via the syntax "name[index]", mapped properties via the syntax "name('key')", nested properties via the syntax "outer.inner" or
        /// combinations thereof.
        /// </summary>
        /// <value></value>
        public object this[string property]
        {
            get { return Get(property); }
        }

        public object Underlying
        {
            get { return _propertyValues; }
            set { _propertyValues = (Object[])value; }
        }

        public Object GetFragment(String propertyExpression)
        {
            var getter = _eventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }
            return getter.GetFragment(this);
        }
    }
}
