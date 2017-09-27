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

using Avro;

using NEsper.Avro.Core;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanPropertyWriter : EventPropertyWriter
    {
        protected readonly Field Field;

        public AvroEventBeanPropertyWriter(Field field)
        {
            Field = field;
        }

        public void Write(Object value, EventBean target)
        {
            var avroEvent = (AvroGenericDataBackedEventBean) target;
            Write(value, avroEvent.Properties);
        }

        public virtual void Write(Object value, GenericRecord record)
        {
            record.Add(Field.Name, value);
        }
    }
} // end of namespace