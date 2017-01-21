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
    /// A getter that works on arrays residing within a Map as an event property.
    /// </summary>
    public class ObjectArrayArrayPONOEntryIndexedPropertyGetter
        : BaseNativePropertyGetter
          ,
          ObjectArrayEventPropertyGetterAndIndexed
    {
        private readonly int _index;
        private readonly int _propertyIndex;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyIndex">MapIndex of the property.</param>
        /// <param name="index">the index to fetch the array element for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="returnType">type of the entry returned</param>
        public ObjectArrayArrayPONOEntryIndexedPropertyGetter(int propertyIndex,
                                                              int index,
                                                              EventAdapterService eventAdapterService,
                                                              Type returnType)
            : base(eventAdapterService, returnType, null)
        {
            _propertyIndex = propertyIndex;
            _index = index;
        }

        #region ObjectArrayEventPropertyGetterAndIndexed Members

        public Object GetObjectArray(Object[] array)
        {
            return GetArrayInternal(array, _index);
        }

        public bool IsObjectArrayExistsProperty(Object[] array)
        {
            return array.Length > _index;
        }

        public Object Get(EventBean eventBean, int index)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetArrayInternal(array, index);
        }

        public override Object Get(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return GetObjectArray(array);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            Object[] array = BaseNestableEventUtil.CheckedCastUnderlyingObjectArray(eventBean);
            return array.Length > _index;
        }

        #endregion

        public Object GetArrayInternal(Object[] array, int index)
        {
            // If the map does not contain the key, this is allowed and represented as null
            Object value = array[_propertyIndex];
            return BaseNestableEventUtil.GetIndexedValue(value, index);
        }
    }
}