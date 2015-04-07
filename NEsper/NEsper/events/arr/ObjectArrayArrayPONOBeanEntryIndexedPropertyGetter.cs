///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public class ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter
        : BaseNativePropertyGetter
          ,
          ObjectArrayEventPropertyGetter
    {
        private readonly int _index;
        private readonly BeanEventPropertyGetter _nestedGetter;
        private readonly int _propertyIndex;

        /// <summary>Ctor. </summary>
        /// <param name="propertyIndex">the property to look at</param>
        /// <param name="nestedGetter">the getter for the map entry</param>
        /// <param name="eventAdapterService">for producing wrappers to objects</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="returnType">type of the entry returned</param>
        public ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter(int propertyIndex,
                                                                  int index,
                                                                  BeanEventPropertyGetter nestedGetter,
                                                                  EventAdapterService eventAdapterService,
                                                                  Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyIndex = propertyIndex;
            _index = index;
            _nestedGetter = nestedGetter;
        }

        #region ObjectArrayEventPropertyGetter Members

        public Object GetObjectArray(Object[] array)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = array[_propertyIndex];
            return BaseNestableEventUtil.GetBeanArrayValue(_nestedGetter, value, _index);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
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

        #endregion
    }
}