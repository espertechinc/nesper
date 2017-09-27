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
    public class AvroEventBeanGetterNestedIndexRooted : EventPropertyGetter
    {
        private readonly int _index;
        private readonly AvroEventPropertyGetter _nested;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedIndexRooted(Field posTop, int index, AvroEventPropertyGetter nested)
        {
            _posTop = posTop;
            _index = index;
            _nested = nested;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (ICollection<object>) record.Get(_posTop);
            var value = AvroEventBeanGetterIndexed.GetIndexedValue(values, _index);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return _nested.GetAvroFieldValue((GenericRecord) value);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = (ICollection<object>) record.Get(_posTop);
            var value = AvroEventBeanGetterIndexed.GetIndexedValue(values, _index);
            if (value == null || !(value is GenericRecord))
            {
                return null;
            }
            return _nested.GetAvroFragment((GenericRecord) value);
        }
    }
} // end of namespace