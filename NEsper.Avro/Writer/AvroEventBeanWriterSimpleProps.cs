///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events;

using Avro;
using Avro.Generic;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly IList<Field> _fields;

        public AvroEventBeanWriterSimpleProps(IList<Field> fields)
        {
            _fields = fields;
        }

        public void Write(Object[] values, EventBean theEvent)
        {
            var row = (GenericRecord) theEvent.Underlying;
            for (int i = 0; i < values.Length; i++)
            {
                row.Add(_fields[i].Name, values[i]);
            }
        }
    }
} // end of namespace