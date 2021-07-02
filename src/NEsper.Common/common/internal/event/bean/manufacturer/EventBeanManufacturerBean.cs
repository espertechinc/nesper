///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.instantiator;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerBean : EventBeanManufacturer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventType _beanEventType;

        private readonly BeanInstantiator _beanInstantiator;
        private readonly bool _hasPrimitiveTypes;
        private readonly bool[] _primitiveType;
        private readonly EventBeanTypedEventFactory _service;
        private readonly MemberInfo[] _writeMembersReflection;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="beanEventType">target type</param>
        /// <param name="service">factory for events</param>
        /// <param name="properties">written properties</param>
        /// <param name="importService">for resolving write methods</param>
        /// <throws>EventBeanManufactureException if the write method lookup fail</throws>
        public EventBeanManufacturerBean(
            BeanEventType beanEventType,
            EventBeanTypedEventFactory service,
            WriteablePropertyDescriptor[] properties,
            ImportService importService)
        {
            this._beanEventType = beanEventType;
            this._service = service;

            _beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, importService)
                .BeanInstantiator;

            _writeMembersReflection = new MemberInfo[properties.Length];
            var primitiveTypeCheck = false;
            _primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                _writeMembersReflection[i] = properties[i].WriteMember;
                _primitiveType[i] = properties[i].PropertyType.IsValueType;
                primitiveTypeCheck |= _primitiveType[i];
            }

            _hasPrimitiveTypes = primitiveTypeCheck;
        }

        public EventBean Make(object[] propertyValues)
        {
            var outObject = MakeUnderlying(propertyValues);
            return _service.AdapterForTypedObject(outObject, _beanEventType);
        }

        public object MakeUnderlying(object[] propertyValues)
        {
            var outObject = _beanInstantiator.Instantiate();

            if (!_hasPrimitiveTypes) {
                var parameters = new object[1];
                for (var i = 0; i < _writeMembersReflection.Length; i++) {
                    parameters[0] = propertyValues[i];
                    try {
                        var writeMember = _writeMembersReflection[i];
                        if (writeMember is MethodInfo writeMethod) {
                            writeMethod.Invoke(outObject, parameters);
                        }
                        else if (writeMember is PropertyInfo writeProperty) {
                            writeProperty.SetValue(outObject, parameters[0]);
                        }
                        else {
                            throw new IllegalStateException("writeMember of invalid type");
                        }
                    }
                    catch (MemberAccessException e) {
                        Handle(e, _writeMembersReflection[i].Name);
                    }
                    catch (Exception e) {
                        var message = "Unexpected exception encountered invoking setter-method '" +
                                      _writeMembersReflection[i] +
                                      "' on class '" +
                                      _beanEventType.UnderlyingType.Name +
                                      "' : " +
                                      e.Message;
                        Log.Error(message, e);
                    }
                }
            }
            else {
                var parameters = new object[1];
                for (var i = 0; i < _writeMembersReflection.Length; i++) {
                    if (_primitiveType[i] && propertyValues[i] == null) {
                        continue;
                    }

                    parameters[0] = propertyValues[i];
                    try {
                        var writeMember = _writeMembersReflection[i];
                        if (writeMember is MethodInfo writeMethod) {
                            writeMethod.Invoke(outObject, parameters);
                        }
                        else if (writeMember is PropertyInfo writeProperty) {
                            writeProperty.SetValue(outObject, parameters[0]);
                        }
                        else {
                            throw new IllegalStateException("invalid member info");
                        }
                    }
                    catch (MemberAccessException e) {
                        Handle(e, _writeMembersReflection[i].Name);
                    }
                    catch (Exception e) {
                        HandleAny(e, _writeMembersReflection[i].Name);
                    }
                }
            }

            return outObject;
        }

        private void HandleAny(
            Exception ex,
            string methodName)
        {
            var message = "Unexpected exception encountered invoking setter-method '" +
                          methodName +
                          "' on class '" +
                          _beanEventType.UnderlyingType.Name +
                          "' : " +
                          ex.Message;
            Log.Error(message, ex);
        }

        private void Handle(
            MemberAccessException ex,
            string methodName)
        {
            var message = "Unexpected exception encountered invoking setter-method '" +
                          methodName +
                          "' on class '" +
                          _beanEventType.UnderlyingType.Name +
                          "' : " +
                          ex.Message;
            Log.Error(message, ex);
        }
    }
} // end of namespace