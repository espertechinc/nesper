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
    public class AvroEventBeanGetterNestedIndexRootedMultilevel : EventPropertyGetter
    {
        private readonly int _index;
        private readonly AvroEventPropertyGetter[] _nested;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedIndexRootedMultilevel(Field posTop, int index, AvroEventPropertyGetter[] nested)
        {
            _posTop = posTop;
            _index = index;
            _nested = nested;
        }

        public Object Get(EventBean eventBean)
        {
            Object value = Navigate(eventBean);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return _nested[_nested.Length - 1].GetAvroFieldValue((GenericRecord) value);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            Object value = Navigate(eventBean);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return _nested[_nested.Length - 1].GetAvroFragment((GenericRecord) value);
        }

        private Object Navigate(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (ICollection<object>) record.Get(_posTop);
            Object value = AvroEventBeanGetterIndexed.GetIndexedValue(values, _index);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            for (int i = 0; i < _nested.Length - 1; i++)
            {
                value = _nested[i].GetAvroFieldValue((GenericRecord) value);
                if (value == null || !(value is GenericRecord))
                {
                    return null;
                }
            }
            return value;
        }
    }
} // end of namespace