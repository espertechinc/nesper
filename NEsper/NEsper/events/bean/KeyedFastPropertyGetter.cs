///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using XLR8.CGLib;
using com.espertech.esper.client;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a key property identified by a given key value, using the CGLIB fast
    /// method.
    /// </summary>
    public class KeyedFastPropertyGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndMapped
        , EventPropertyGetterAndIndexed
    {
        private readonly FastMethod _fastMethod;
        private readonly Object _key;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fastMethod">is the method to use to retrieve a value from the object.</param>
        /// <param name="key">is the key to supply as parameter to the mapped property getter</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public KeyedFastPropertyGetter(FastMethod fastMethod, Object key, EventAdapterService eventAdapterService)
            : base(eventAdapterService, fastMethod.ReturnType, null)
        {
            _key = key;
            _fastMethod = fastMethod;
        }

        public bool IsBeanExistsProperty(Object o)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean eventBean) {
            return GetBeanProp(eventBean.Underlying);
        }

        public Object GetBeanProp(Object o) {
            return GetBeanPropInternal(o, _key);
        }

        public Object Get(EventBean eventBean, String mapKey) {
            return GetBeanPropInternal(eventBean.Underlying, mapKey);
        }

        public Object Get(EventBean eventBean, int index) {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }

        public Object GetBeanPropInternal(Object o, Object key)
        {
            try
            {
                return _fastMethod.Invoke(o, key);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_fastMethod.Target, o, e);
            }
            catch (PropertyAccessException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new PropertyAccessException(e);
            }
        }

        public override String ToString()
        {
            return "KeyedFastPropertyGetter " +
                    " fastMethod=" + _fastMethod +
                    " key=" + _key;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
