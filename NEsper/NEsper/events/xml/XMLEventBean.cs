///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    /// <summary>
    /// EventBean wrapper for XML documents.  Currently only instances of System.Xml.XmlNode can be used
    /// </summary>

    public class XMLEventBean : EventBeanSPI
    {
        private XmlNode _event;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="theEvent">is the node with event property information</param>
        /// <param name="type">is the event type for this event wrapper</param>

        public XMLEventBean(XmlNode theEvent, EventType type)
        {
            _event = theEvent;
            EventType = type;
        }

        /// <summary>
        /// Return the <see cref="EventType" /> instance that describes the set of properties available for this event.
        /// </summary>
        /// <value></value>
        /// <returns> event type
        /// </returns>
        public EventType EventType { get; private set; }

        /// <summary>
        /// Get the underlying data object to this event wrapper.
        /// </summary>
        /// <value></value>
        /// <returns> underlying data object, usually either a Map or a bean instance.
        /// </returns>
        public Object Underlying
        {
            get { return _event; }
            set { _event = value as XmlNode; }
        }

        /// <summary>
        /// Returns the value of an event property.
        /// </summary>
        /// <value></value>
        /// <returns> the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public Object this[String property]
        {
            get
            {
                var getter = EventType.GetGetter(property);
                if (getter == null)
                {
                    throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
                }

                return getter.Get(this);
            }
        }

        /// <summary>
        /// Returns the value of an event property.  This method is a proxy of the indexer.
        /// </summary>
        /// <param name="property">name of the property whose value is to be retrieved</param>
        /// <returns>
        /// the value of a simple property with the specified name.
        /// </returns>
        /// <throws>  PropertyAccessException - if there is no property of the specified name, or the property cannot be accessed </throws>
        public Object Get(String property)
        {
            var getter = EventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException("Property named '" + property + "' is not a valid property name for this type");
            }

            return getter.Get(this);
        }

        public Object GetFragment(String propertyExpression)
        {
            EventPropertyGetter getter = EventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw new PropertyAccessException(
                    "Property named '" + propertyExpression + "' is not a valid property name for this type");
            }
            return getter.GetFragment(this);
        }
    }
}
