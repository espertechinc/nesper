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
    using DataMap = IDictionary<object, object>;

    /// <summary>
    /// Getter for a key property identified by a given key value, using vanilla reflection.
    /// </summary>
    public class KeyedMapMethodPropertyGetter 
        : BaseNativePropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly MethodInfo _method;
        private readonly Object _key;
    
        /// <summary>Constructor. </summary>
        /// <param name="method">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapMethodPropertyGetter(MethodInfo method, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnTypeMap(method, false), null)
        {
            _key = key;
            _method = method;
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
                Object result = _method.Invoke(@object, null);
                if (!(result is DataMap)) {
                    return null;
                }
                DataMap resultMap = (DataMap)result;
                return resultMap.Get(key);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_method, @object, e);
            }
            catch (TargetInvocationException e)
            {
                throw PropertyUtility.GetInvocationTargetException(_method, e);
            }
            catch (ArgumentException e)
            {
                throw new PropertyAccessException(e);
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
            return "KeyedMapMethodPropertyGetter " +
                    " method=" + _method +
                    " key=" + _key;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
