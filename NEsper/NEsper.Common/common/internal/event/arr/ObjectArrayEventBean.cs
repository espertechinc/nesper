///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class ObjectArrayEventBean : EventBeanSPI,
        ObjectArrayBackedEventBean
    {
        public ObjectArrayEventBean(object[] propertyValues, EventType eventType)
        {
            Properties = propertyValues;
            EventType = eventType;
        }

        public EventType EventType { get; set; }

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

        public object Underlying {
            get => Properties;
            set => Properties = (object[]) value;
        }

        public object GetFragment(string propertyExpression)
        {
            var getter = EventType.GetGetter(propertyExpression);
            if (getter == null) {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }

            return getter.GetFragment(this);
        }

        public object[] Properties { get; set; }

        public object[] PropertyValues {
            get => Properties;
            set => Properties = value;
        }

        public object UnderlyingSpi {
            get => Underlying;
            set => Underlying = value;
        }
    }
} // end of namespace