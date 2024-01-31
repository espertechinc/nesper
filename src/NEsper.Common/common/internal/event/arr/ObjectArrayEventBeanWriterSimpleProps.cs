///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Writer method for writing to Object-Array-type events.
    /// </summary>
    public class ObjectArrayEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly int[] indexes;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="indexes">indexes of properties to write</param>
        public ObjectArrayEventBeanWriterSimpleProps(int[] indexes)
        {
            this.indexes = indexes;
        }

        /// <summary>
        ///     Write values to an event.
        /// </summary>
        /// <param name="values">to write</param>
        /// <param name="theEvent">to write to</param>
        public void Write(
            object[] values,
            EventBean theEvent)
        {
            var arrayEvent = (ObjectArrayBackedEventBean)theEvent;
            var array = arrayEvent.Properties;

            for (var i = 0; i < indexes.Length; i++) {
                array[indexes[i]] = values[i];
            }
        }
    }
} // end of namespace