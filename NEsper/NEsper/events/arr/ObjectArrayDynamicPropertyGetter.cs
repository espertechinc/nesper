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
    /// <summary>
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class ObjectArrayDynamicPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly string _propertyName;
    
        public ObjectArrayDynamicPropertyGetter(string propertyName)
        {
            _propertyName = propertyName;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            return null;
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return false;
        }
    
        public Object Get(EventBean eventBean)
        {
            int index;

            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;
            if (!objectArrayEventType.PropertiesIndexes.TryGetValue(_propertyName, out index))
            {
                return null;
            }

            return ((Object[]) eventBean.Underlying)[index];
        }
    
        public bool IsExistsProperty(EventBean eventBean)
        {
            int index;

            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;
            if (objectArrayEventType.PropertiesIndexes.TryGetValue(_propertyName, out index))
            {
                return true;
            }

            return false;
        }
    
        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
} // end of namespace
