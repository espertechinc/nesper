///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly Field[] _positions;

        public AvroEventBeanWriterSimpleProps(Field[] positions)
        {
            _positions = positions;
        }

        public void Write(
            object[] values,
            EventBean theEvent)
        {
            var row = (GenericRecord) theEvent.Underlying;
            for (var i = 0; i < values.Length; i++)
            {
                row.Put(_positions[i], values[i]);
            }
        }
    }
} // end of namespace