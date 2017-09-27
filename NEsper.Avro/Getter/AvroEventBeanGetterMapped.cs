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
using com.espertech.esper.compat.collections;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;
using com.espertech.esper.compat.magic;

namespace NEsper.Avro.Getter
{
    //using Map = IDictionary<string, object>;

    public class AvroEventBeanGetterMapped : AvroEventPropertyGetter
    {
        private readonly string _key;
        private readonly Field _pos;

        public AvroEventBeanGetterMapped(Field pos, string key)
        {
            _pos = pos;
            _key = key;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_pos);
            return GetMappedValue(values, _key);
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            var values = record.Get(_pos);
            return GetMappedValue(values, _key);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            return null;
        }

        public static Object GetMappedValue(object map, string key)
        {
            if (map == null)
            {
                return null;
            }

            var magicMap = MagicMarker.GetStringDictionary(map);
            return magicMap.Get(key);
        }
    }
} // end of namespace