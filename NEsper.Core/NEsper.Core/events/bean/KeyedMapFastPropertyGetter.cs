///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.collections;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    using DataMap = IDictionary<string, object>;

    /// <summary>
    /// Getter for a key property identified by a given key value of a map, using the CGLIB fast method.
    /// </summary>
    public class KeyedMapFastPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
    {
        private readonly FastMethod _fastMethod;
        private readonly String _key;

        /// <summary>Constructor. </summary>
        /// <param name="method">the underlying method</param>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedMapFastPropertyGetter(MethodInfo method, FastMethod fastMethod, String key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, TypeHelper.GetGenericReturnTypeMap(method, false), null)
        {
            _key = key;
            _fastMethod = fastMethod;
        }
    
        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _key);
        }
    
        public Object Get(EventBean eventBean, String mapKey)
        {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }
    
        public Object GetBeanPropInternal(Object @object, String key)
        {
            try
            {
                var result = _fastMethod.Invoke(@object, null) ;
                return GenericExtensions.FetchKeyedValue(result, key, null);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, @object, e);
            }
            catch (TargetInvocationException e)
            {
                throw PropertyUtility.GetInvocationTargetException(_fastMethod.Target, e);
            }
        }
    
        public override Object Get(EventBean eventBean)
        {
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }
    
        public override String ToString()
        {
            return "KeyedMapFastPropertyGetter " +
                    " fastMethod=" + _fastMethod +
                    " key=" + _key;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
