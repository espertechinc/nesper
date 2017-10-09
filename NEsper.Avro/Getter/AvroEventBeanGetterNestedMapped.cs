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

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterNestedMapped : EventPropertyGetter
    {
        private readonly string _key;
        private readonly Field _pos;
        private readonly Field _top;

        public AvroEventBeanGetterNestedMapped(Field top, Field pos, string key)
        {
            _top = top;
            _pos = pos;
            _key = key;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            if (inner == null)
            {
                return null;
            }
            var map = (Map) inner.Get(_pos);
            return AvroEventBeanGetterMapped.GetMappedValue(map, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
} // end of namespace