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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;
using com.espertech.esper.events.vaevent;

namespace com.espertech.esper.events.bean
{
    using DataMap = System.Collections.Generic.IDictionary<string, object>;

    /// <summary>
    /// Getter for a dynamic mapped property (syntax field.Mapped('key')?), using
    /// vanilla reflection.
    /// </summary>
    public class DynamicMappedPropertyGetter
        : DynamicPropertyGetterBase
    {
        private readonly String _propertyName;
        private readonly String[] _paramList;
        private readonly EventAdapterService _eventAdapterService;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fieldName">property name</param>
        /// <param name="key">mapped access key</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        public DynamicMappedPropertyGetter(String fieldName, String key, EventAdapterService eventAdapterService)
            : base(eventAdapterService)
        {
            _propertyName = fieldName;
            _paramList = new[] { key };
            _eventAdapterService = eventAdapterService;
        }

        protected override MethodInfo DetermineMethod(Type type)
        {
            var property = MagicType
                .GetCachedType(type)
                .ResolveProperty(_propertyName, _eventAdapterService.DefaultPropertyResolutionStyle);

            if (property == null)
                return null;

            if (property.EventPropertyType == EventPropertyType.SIMPLE)
            {
                if (property.GetMethod.ReturnType.IsGenericStringDictionary())
                {
                    return property.GetMethod;
                }
            }
            else if (property.EventPropertyType == EventPropertyType.MAPPED)
            {
                return property.GetMethod;
            }

            return null;
        }

        private Type _lastResultType;
        private Func<object, DataMap> _lastResultFactory;

        protected override Object Call(DynamicPropertyDescriptor descriptor, Object underlying)
        {
            try
            {
                if (descriptor.HasParameters)
                {
                    return descriptor.Method.Invoke(underlying, _paramList);
                }

                var result = descriptor.Method.Invoke(underlying, null);
                if ( result == null ) {
                    return null;
                }

                if (result is DataMap) {
                    var resultDictionary = (DataMap) result;
                    return resultDictionary.Get(_paramList[0]);
                }

                Func<object, DataMap> resultFactory = null;

                var resultType = result.GetType();
                if (ReferenceEquals(resultType, _lastResultType)) {
                    resultFactory = _lastResultFactory;
                } else {
                    _lastResultFactory = resultFactory = MagicMarker.GetStringDictionaryFactory(resultType);
                    _lastResultType = resultType;
                }
                
                if (resultFactory != null) {
                    var resultDictionary = resultFactory.Invoke(result);
                    return resultDictionary.Get(_paramList[0]);
                }

                return null;
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
