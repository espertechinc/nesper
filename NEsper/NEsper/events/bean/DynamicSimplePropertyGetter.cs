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
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class DynamicSimplePropertyGetter : DynamicPropertyGetterBase 
    {
        private readonly string _propertyName;
        private readonly EventAdapterService _adapterService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fieldName">the property name</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public DynamicSimplePropertyGetter(String fieldName, EventAdapterService eventAdapterService)
            : base(eventAdapterService)
        {
            _adapterService = eventAdapterService;
            _propertyName = fieldName;
        }
    
        protected override Object Call(DynamicPropertyDescriptor descriptor, Object underlying)
        {
            try
            {
                return descriptor.Method.Invoke(underlying, null);
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

        protected override MethodInfo DetermineMethod(Type type)
        {
            return MagicType
                .GetCachedType(type)
                .ResolvePropertyMethod(_propertyName, _adapterService.DefaultPropertyResolutionStyle);
        }
    }
}
