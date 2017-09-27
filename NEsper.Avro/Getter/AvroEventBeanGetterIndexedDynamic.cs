///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexedDynamic : AvroEventPropertyGetter
    {
        private readonly string _propertyName;
        private readonly int _index;

        public AvroEventBeanGetterIndexedDynamic(string propertyName, int index)
        {
            _propertyName = propertyName;
            _index = index;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            var value = record.Get(_propertyName);
            if (value == null)
            {
                return null;
            }

            if (value is Array)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue((Array) value, _index);
            }

            if (value is IEnumerable<object>)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue((IEnumerable<object>) value, _index);
            }

            if (value is IEnumerable)
            {
                return AvroEventBeanGetterIndexed.GetIndexedValue(((IEnumerable) value).Cast<object>(), _index);
            }

            return null;
        }

        public Object Get(EventBean eventBean)
        {
            GenericRecord record = (GenericRecord) eventBean.Underlying;
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
            var value = record.Get(_propertyName);
            return ((value == null)
                 || (value is Array)
                 || (value is IEnumerable<object>)
                 || (value is IEnumerable));
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
