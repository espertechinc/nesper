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
    /// Property getter for fields using reflection.
    /// </summary>
    public sealed class ReflectionPropFieldGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
    {
        private readonly FieldInfo field;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="field">is the regular reflection field to use to obtain values for a property</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public ReflectionPropFieldGetter(FieldInfo field, EventAdapterService eventAdapterService)
            : base(eventAdapterService, field.FieldType, TypeHelper.GetGenericFieldType(field, true))
        {
            this.field = field;
        }

        public Object GetBeanProp(Object o)
        {
            try
            {
                return field.GetValue(o);
            }
            catch (ArgumentException e)
            {
                throw PropertyUtility.GetIllegalArgumentException(field, e);
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
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public override Object Get(EventBean eventBean)
        {
            Object underlying = eventBean.Underlying;
            return GetBeanProp(underlying);
        }

        public override String ToString()
        {
            return "ReflectionPropFieldGetter " +
                    "field=" + field.ToString();
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
