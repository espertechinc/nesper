///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.events.arr
{
    public class ObjectArrayEventBeanPropertyWriterIndexedProp 
        : ObjectArrayEventBeanPropertyWriter
    {
        private readonly int _indexTarget;
    
        public ObjectArrayEventBeanPropertyWriterIndexedProp(int propertyIndex, int indexTarget)
            : base(propertyIndex)
        {
            _indexTarget = indexTarget;
        }
    
        public override void Write(Object value, Object[] array)
        {
            var arrayEntry = array[Index] as Array;
            if (arrayEntry != null && arrayEntry.Length > _indexTarget)
            {
                arrayEntry.SetValue(value, _indexTarget);
            }
        }
    }
}
