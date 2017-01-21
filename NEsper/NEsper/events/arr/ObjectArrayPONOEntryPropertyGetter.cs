///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.events.arr
{
    /// <summary>
    /// A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayPONOEntryPropertyGetter
        : BaseNativePropertyGetter
        , ObjectArrayEventPropertyGetter
    {
        private readonly int _propertyIndex;
        private readonly BeanEventPropertyGetter _entryGetter;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">MapIndex of the property.</param>
        /// <param name="entryGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="returnType">type of the entry returned</param>
        /// <param name="nestedComponentType">Type of the nested component.</param>
        public ObjectArrayPONOEntryPropertyGetter(int propertyIndex, BeanEventPropertyGetter entryGetter, EventAdapterService eventAdapterService, Type returnType, Type nestedComponentType)
            : base(eventAdapterService, returnType, nestedComponentType)
        {
            _propertyIndex = propertyIndex;
            _entryGetter = entryGetter;
        }
    
        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            var value = array[_propertyIndex];
            if (value == null)
            {
                return null;
            }
    
            // Object within the map
            if (value is EventBean)
            {
                return _entryGetter.Get((EventBean) value);
            }

            return _entryGetter.GetBeanProp(value);
        }
    
        public bool IsObjectArrayExistsProperty(Object[] array) {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
