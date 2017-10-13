///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Property getter for methods using vanilla reflection.
    /// </summary>
    public sealed class ReflectionPropMethodGetter
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly MethodInfo _method;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="method">is the regular reflection method to use to obtain values for a field.</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ReflectionPropMethodGetter(MethodInfo method, EventAdapterService eventAdapterService)
            : base(eventAdapterService, method.ReturnType, TypeHelper.GetGenericReturnType(method, false))
        {
            _method = method;
        }

        public Object GetBeanProp(Object o)
        {
            try
            {
                return _method.Invoke(o, null);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(_method, e);
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

        public bool IsBeanExistsProperty(Object o)
        {
            return true;
        }

        public override Object Get(EventBean eventBean)
        {
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "ReflectionPropMethodGetter " +
                    "method=" + _method;
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
