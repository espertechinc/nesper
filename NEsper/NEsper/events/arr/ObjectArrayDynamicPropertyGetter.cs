///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class ObjectArrayDynamicPropertyGetter : ObjectArrayEventPropertyGetter {
        private readonly string propertyName;
    
        public ObjectArrayDynamicPropertyGetter(string propertyName) {
            this.propertyName = propertyName;
        }
    
        public Object GetObjectArray(Object[] array) {
            return null;
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array) {
            return false;
        }
    
        public Object Get(EventBean eventBean) {
            ObjectArrayEventType objectArrayEventType = (ObjectArrayEventType) eventBean.EventType;
            int? index = objectArrayEventType.PropertiesIndexes.Get(propertyName);
            if (index == null) {
                return null;
            }
            Object[] theEvent = (Object[]) eventBean.Underlying;
            return theEvent[index];
        }
    
        public bool IsExistsProperty(EventBean eventBean) {
            ObjectArrayEventType objectArrayEventType = (ObjectArrayEventType) eventBean.EventType;
            int? index = objectArrayEventType.PropertiesIndexes.Get(propertyName);
            return index != null;
        }
    
        public Object GetFragment(EventBean eventBean) {
            return null;
        }
    }
} // end of namespace
