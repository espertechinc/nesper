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

using NEsper.Avro.Core;

namespace NEsper.Avro.Writer
{
    /// <summary>Writer method for writing to Object-array-type events.</summary>
    public class AvroEventBeanWriterPerProp : EventBeanWriter
    {
        private readonly AvroEventBeanPropertyWriter[] _writers;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="writers">names of properties to write</param>
        public AvroEventBeanWriterPerProp(AvroEventBeanPropertyWriter[] writers)
        {
            _writers = writers;
        }

        /// <summary>
        ///     Write values to an event.
        /// </summary>
        /// <param name="values">to write</param>
        /// <param name="theEvent">to write to</param>
        public void Write(Object[] values, EventBean theEvent)
        {
            var arrayEvent = (AvroGenericDataBackedEventBean) theEvent;
            GenericRecord arr = arrayEvent.Properties;

            for (int i = 0; i < _writers.Length; i++)
            {
                _writers[i].Write(values[i], arr);
            }
        }
    }
} // end of namespace