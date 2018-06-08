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
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Getter for a dynamic indexed property (syntax field.indexed[0]?), using vanilla
    /// reflection.
    /// </summary>
    public class DynamicIndexedPropertyGetter : DynamicPropertyGetterBase
    {
        private readonly String _propertyName;
        private readonly Object[] _paramList;
        private readonly int _index;
        private readonly EventAdapterService _eventAdapterService;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="index">index to get the element at</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public DynamicIndexedPropertyGetter(String propertyName, int index, EventAdapterService eventAdapterService)
            : base(eventAdapterService)
        {
            _propertyName = propertyName;
            _paramList = new Object[] { index };
            _eventAdapterService = eventAdapterService;
            _index = index;
        }

        protected override MethodInfo DetermineMethod(Type type)
        {
            var property = MagicType
                .GetCachedType(type)
                .ResolveProperty(_propertyName, _eventAdapterService.DefaultPropertyResolutionStyle);

            if (property == null)
                return null;

            if (property.EventPropertyType == EventPropertyType.SIMPLE) {
                if (property.GetMethod.ReturnType.IsArray) {
                    return property.GetMethod;
                }
            }
            else if (property.EventPropertyType == EventPropertyType.INDEXED) {
                return property.GetMethod;
            }

            return null;
        }

        protected override Object Call(DynamicPropertyDescriptor descriptor, Object underlying)
        {
            try
            {
                if (descriptor.HasParameters)
                {
                    return descriptor.Method.Invoke(underlying, _paramList);
                }

                var array = (Array) descriptor.Method.Invoke(underlying, null);
                if (array == null)
                {
                    return null;
                }
                if (array.Length <= _index)
                {
                    return null;
                }
                return array.GetValue(_index);
            }
            catch (InvalidCastException e)
            {
                throw PropertyUtility.GetMismatchException(descriptor.Method.Target, underlying, e);
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
    }
}
