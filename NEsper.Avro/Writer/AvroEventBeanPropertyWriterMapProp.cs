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

using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    using Map = IDictionary<string, object>;

    public class AvroEventBeanPropertyWriterMapProp : AvroEventBeanPropertyWriter
    {
        private readonly string key;

        public AvroEventBeanPropertyWriterMapProp(Field propertyField, string key)
            : base(propertyField)
        {
            this.key = key;
        }

        public override void Write(Object value, GenericRecord record)
        {
            Object val = record.Get(Field);
            if (val != null && val is Map)
            {
                var map = (Map) val;
                map.Put(key, value);
            }
        }
    }
} // end of namespace