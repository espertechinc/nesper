///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace NEsper.Avro.Core
{
    public class AvroGenericDataEventBean : EventBean,
        AvroGenericDataBackedEventBean,
        AvroBackedBean
    {
        public AvroGenericDataEventBean(
            GenericRecord record,
            EventType eventType)
        {
            Properties = record;
            EventType = eventType;
        }

        public GenericRecord Properties { get; private set; }

        public object GenericRecordDotData {
            get => Properties;
            set => Properties = (GenericRecord) value;
        }

        public EventType EventType { get; }

        public object Underlying {
            get => Properties;
            set => throw new NotSupportedException();
        }

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

        public object this[string property] => Get(property);
    }
} // end of namespace