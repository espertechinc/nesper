///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events.arr
{
    public class ObjectArrayEventBeanPropertyWriter : EventPropertyWriter
    {
        protected readonly int Index;

        public ObjectArrayEventBeanPropertyWriter(int index)
        {
            Index = index;
        }

        public virtual void Write(Object value, EventBean target)
        {
            var arrayEvent = (ObjectArrayBackedEventBean)target;
            Write(value, arrayEvent.Properties);
        }

        public virtual void Write(Object value, Object[] array)
        {
            array[Index] = value;
        }
    }
}
