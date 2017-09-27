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

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a string property backed by a field, identified by a given index, using
    /// vanilla reflection.
    /// </summary>
    public class StringFieldPropertyGetter 
        : BaseNativePropertyGetter
        , BeanEventPropertyGetter
        , EventPropertyGetterAndIndexed
    {
        private readonly FieldInfo _field;
        private readonly int _index;
    
        /// <summary>Constructor. </summary>
        /// <param name="field">is the field to use to retrieve a value from the object</param>
        /// <param name="index">is tge index within the array to get the property from</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public StringFieldPropertyGetter(FieldInfo field, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService, typeof(char), null)
        {
            _index = index;
            _field = field;
    
            if (index < 0)
            {
                throw new ArgumentException("Invalid negative index value");
            }
        }
    
        public Object GetBeanProp(Object @object)
        {
            return GetBeanPropInternal(@object, _index);
        }
    
        private Object GetBeanPropInternal(Object @object, int index)
        {
           try
            {
                var value = (String) _field.GetValue(@object);
                if (value.Length <= index)
                {
                    return null;
                }

                return value[index];
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(_field, @object, e);
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
    
        public bool IsBeanExistsProperty(Object @object)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    
        public override Object Get(EventBean eventBean)
        {
            return GetBeanProp(eventBean.Underlying);
        }
    
        public Object Get(EventBean eventBean, int index)
        {
            return GetBeanPropInternal(eventBean.Underlying, index);
        }
    
        public override String ToString()
        {
            return "StringFieldPropertyGetter " +
                    " field=" + _field +
                    " index=" + _index;
        }
    
        public override bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }
    }
}
