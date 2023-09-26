///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     EventBean wrapper for XML documents.
    /// </summary>
    public class XMLEventBean : EventBeanSPI
    {
        private XmlNode _theEvent;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="theEvent">is the node with event property information</param>
        /// <param name="type">is the event type for this event wrapper</param>
        public XMLEventBean(
            XmlNode theEvent,
            EventType type)
        {
            _theEvent = theEvent;
            EventType = type;
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

        public object this[string property] => Get(property);

        public object UnderlyingSpi {
            get => _theEvent;
            set => _theEvent = (XmlNode)value;
        }

        public object Underlying {
            get => _theEvent;
            set => _theEvent = (XmlNode)value;
        }

        public object GetFragment(string propertyExpression)
        {
            var getter = EventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }
    }
} // end of namespace