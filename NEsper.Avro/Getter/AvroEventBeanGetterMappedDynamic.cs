///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterMappedDynamic : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly string _propertyName;

        public AvroEventBeanGetterMappedDynamic(string propertyName, string key)
        {
            _propertyName = propertyName;
            _key = key;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            Object value = record.Get(_propertyName);
            if (value == null || !(value is Map))
            {
                return null;
            }
            return AvroEventBeanGetterMapped.GetMappedValue((Map) value, _key);
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFieldValue(record);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return IsExistsPropertyAvro((GenericRecord) eventBean.Underlying);
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            Field field = record.Schema.GetField(_propertyName);
            if (field == null)
            {
                return false;
            }
            Object value = record.Get(_propertyName);
            return value == null || value is Map;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }
    }
} // end of namespace