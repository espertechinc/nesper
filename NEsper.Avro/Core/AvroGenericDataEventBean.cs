///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events;

namespace NEsper.Avro.Core
{
    public class AvroGenericDataEventBean
        : EventBean
        , AvroGenericDataBackedEventBean
        , AvroBackedBean
    {
        private GenericRecord _record;
        private readonly EventType _eventType;

        public AvroGenericDataEventBean(GenericRecord record, EventType eventType)
        {
            _record = record;
            _eventType = eventType;
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public object Underlying
        {
            get { return _record; }
        }

        public object this[string property]
        {
            get { return Get(property); }
        }

        public Object Get(string property)
        {
            var getter = _eventType.GetGetter(property);
            if (getter == null)
            {
                throw new PropertyAccessException(
                    "Property named '" + property + "' is not a valid property name for this type");
            }
            return getter.Get(this);
        }

        public Object GetFragment(string propertyExpression)
        {
            EventPropertyGetter getter = _eventType.GetGetter(propertyExpression);
            if (getter == null)
            {
                throw PropertyAccessException.NotAValidProperty(propertyExpression);
            }
            return getter.GetFragment(this);
        }

        public GenericRecord Properties
        {
            get { return _record; }
        }

        public object GenericRecordDotData
        {
            get { return _record; }
            set { this._record = (GenericRecord) value; }
        }
    }
} // end of namespace
