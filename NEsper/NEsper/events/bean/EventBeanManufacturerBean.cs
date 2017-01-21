///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerBean : EventBeanManufacturer
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
     
        private readonly BeanInstantiator _beanInstantiator;
        private readonly BeanEventType _beanEventType;
        private readonly EventAdapterService _service;
        private readonly FastMethod[] _writeMethodsFastClass;
        private readonly MethodInfo[] _writeMethodsReflection;
        private readonly bool _hasPrimitiveTypes;
        private readonly bool[] _primitiveType;
    
        /// <summary>Ctor. </summary>
        /// <param name="beanEventType">target type</param>
        /// <param name="service">factory for events</param>
        /// <param name="properties">written properties</param>
        /// <param name="engineImportService">for resolving write methods</param>
        /// <throws>EventBeanManufactureException if the write method lookup fail</throws>
        public EventBeanManufacturerBean(BeanEventType beanEventType,
                                         EventAdapterService service,
                                         WriteablePropertyDescriptor[] properties,
                                         EngineImportService engineImportService)
        {
            _beanEventType = beanEventType;
            _service = service;
    
            _beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, engineImportService);

            _writeMethodsReflection = new MethodInfo[properties.Length];
            if (beanEventType.FastClass != null) {
                _writeMethodsFastClass = new FastMethod[properties.Length];
            }
            else {
                _writeMethodsFastClass = null;
            }
    
            bool primitiveTypeCheck = false;
            _primitiveType = new bool[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                _writeMethodsReflection[i] = properties[i].WriteMethod;
                if (beanEventType.FastClass != null) {
                    _writeMethodsFastClass[i] = beanEventType.FastClass.GetMethod(properties[i].WriteMethod);
                }
    
                _primitiveType[i] = properties[i].PropertyType.IsPrimitive;
                primitiveTypeCheck |= _primitiveType[i];
            }
            _hasPrimitiveTypes = primitiveTypeCheck;
        }
    
        public EventBean Make(Object[] propertyValues)
        {
            Object outObject = MakeUnderlying(propertyValues);
            return _service.AdapterForTypedObject(outObject, _beanEventType);
        }
    
        public Object MakeUnderlying(Object[] propertyValues)
        {
            Object outObject = _beanInstantiator.Instantiate();
    
            if (_writeMethodsFastClass != null) {
                if (!_hasPrimitiveTypes) {
                    var parameters = new Object[1];
                    for (int i = 0; i < _writeMethodsFastClass.Length; i++)
                    {
                        parameters[0] = propertyValues[i];
                        try
                        {
                            _writeMethodsFastClass[i].Invoke(outObject, parameters);
                        }
                        catch (Exception e)
                        {
                            HandleAny(e, _writeMethodsFastClass[i].Name);
                        }
                    }
                }
                else
                {
                    Object[] parameters = new Object[1];
                    for (int i = 0; i < _writeMethodsFastClass.Length; i++)
                    {
                        if (_primitiveType[i]) {
                            if (propertyValues[i] == null) {
                                continue;
                            }
                        }
                        parameters[0] = propertyValues[i];
                        try
                        {
                            _writeMethodsFastClass[i].Invoke(outObject, parameters);
                        }
                        catch (Exception e)
                        {
                            HandleAny(e, _writeMethodsFastClass[i].Name);
                        }
                    }
                }
            }
            else {
    
                if (!_hasPrimitiveTypes) {
                    Object[] parameters = new Object[1];
                    for (int i = 0; i < _writeMethodsReflection.Length; i++)
                    {
                        parameters[0] = propertyValues[i];
                        try
                        {
                            _writeMethodsReflection[i].Invoke(outObject, parameters);
                        }
                        catch (MemberAccessException e)
                        {
                            Handle(e, _writeMethodsReflection[i].Name);
                        }
                        catch (Exception e)
                        {
                            String message = "Unexpected exception encountered invoking setter-method '" + _writeMethodsReflection[i] + "' on class '" +
                                    _beanEventType.UnderlyingType.Name + "' : " + e.InnerException.Message;
                            Log.Error(message, e);
                        }
                    }
                }
                else
                {
                    Object[] parameters = new Object[1];
                    for (int i = 0; i < _writeMethodsReflection.Length; i++)
                    {
                        if (_primitiveType[i]) {
                            if (propertyValues[i] == null) {
                                continue;
                            }
                        }
                        parameters[0] = propertyValues[i];
                        try
                        {
                            _writeMethodsReflection[i].Invoke(outObject, parameters);
                        }
                        catch (MemberAccessException e)
                        {
                            Handle(e, _writeMethodsReflection[i].Name);
                        }
                        catch (Exception e)
                        {
                            HandleAny(e, _writeMethodsReflection[i].Name);
                        }
                    }
                }
            }
    
            return outObject;
        }

        private void HandleAny(Exception ex, String methodName)
        {
            if (ex is TargetInvocationException)
                ex = ex.InnerException;

            String message = "Unexpected exception encountered invoking setter-method '" + methodName + "' on class '" +
                    _beanEventType.UnderlyingType.Name + "' : " + ex.Message;
            Log.Error(message, ex);
        }
    
        private void Handle(MemberAccessException ex, String methodName) {
            String message = "Unexpected exception encountered invoking setter-method '" + methodName + "' on class '" +
                    _beanEventType.UnderlyingType.Name + "' : " + ex.Message;
            Log.Error(message, ex);
        }
    }
}
