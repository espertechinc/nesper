///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Writer method for writing to Object-Array-type events.
    /// </summary>
    public class ObjectArrayEventBeanWriterSimpleProps : EventBeanWriter
    {
        private readonly int[] _indexes;
    
        /// <summary>Ctor. </summary>
        /// <param name="indexes">indexes of properties to write</param>
        public ObjectArrayEventBeanWriterSimpleProps(int[] indexes)
        {
            _indexes = indexes;
        }
    
        /// <summary>Write values to an event. </summary>
        /// <param name="values">to write</param>
        /// <param name="theEvent">to write to</param>
        public void Write(Object[] values, EventBean theEvent)
        {
            var arrayEvent = (ObjectArrayBackedEventBean) theEvent;
            var array = arrayEvent.Properties;
    
            for (int i = 0; i < _indexes.Length; i++)
            {
                array[_indexes[i]] = values[i];
            }
        }
    }
}
