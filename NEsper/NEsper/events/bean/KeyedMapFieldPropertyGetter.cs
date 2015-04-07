///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMapFieldPropertyGetter 
        : BaseNativePropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly FieldInfo _field;
        private readonly Object _key;
    
        /// <summary>Constructor. </summary>
        /// <param name="field">is the field to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapFieldPropertyGetter(FieldInfo field, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericFieldTypeMap(field, false), null)
        {
            _key = key;
            _field = field;
        }
    
        public Object Get(EventBean eventBean, String mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }
    
        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }
    
        public Object GetBeanPropInternal(Object @object, Object key)
        {
            try
            {
                var result = _field.GetValue(@object) as DataMap;
                if (result == null)
                {
                    return null;
                }

                return result.Get(Convert.ToString(key));
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_field, @object, e);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(_field, e);
            }
        }
    
        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean eventBean)
        {
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }
    
        public override String ToString()
        {
            return "KeyedMapFieldPropertyGetter " +
                    " field=" + _field +
                    " key=" + _key;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
