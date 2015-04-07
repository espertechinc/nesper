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
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class ObjectArrayDynamicPropertyGetter : ObjectArrayEventPropertyGetter
    {
        private readonly String _propertyName;

        public ObjectArrayDynamicPropertyGetter(String propertyName)
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
            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;

            int index;
            if (!objectArrayEventType.PropertiesIndexes.TryGetValue(_propertyName, out index))
                return null;

            Object[] theEvent = ((Object[])eventBean.Underlying);
            return theEvent[index];
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            var objectArrayEventType = (ObjectArrayEventType)eventBean.EventType;
            return objectArrayEventType.PropertiesIndexes.ContainsKey(_propertyName);
        }

        public Object GetFragment(EventBean eventBean)
        {
            return null;
        }
    }
}
