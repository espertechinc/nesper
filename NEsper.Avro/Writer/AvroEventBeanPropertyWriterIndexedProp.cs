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

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanPropertyWriterIndexedProp : AvroEventBeanPropertyWriter
    {
        private readonly int _indexTarget;

        public AvroEventBeanPropertyWriterIndexedProp(Field propertyField, int indexTarget)
            : base(propertyField)
        {
            this._indexTarget = indexTarget;
        }

        public override void Write(Object value, GenericRecord record)
        {
            var val = record.Get(Field);
            if (val != null && val is IList<object>)
            {
                var list = (IList<object>)val;
                if (list.Count > _indexTarget)
                {
                    list[_indexTarget] = value;
                }
            }
        }
    }
} // end of namespace
