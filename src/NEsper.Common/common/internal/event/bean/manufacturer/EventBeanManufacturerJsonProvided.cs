///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.instantiator;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    /// Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerJsonProvided : EventBeanManufacturer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EventBeanManufacturerJsonProvided));

        private readonly BeanInstantiator _beanInstantiator;
        private readonly JsonEventType _jsonEventType;
        private readonly EventBeanTypedEventFactory _service;
        private readonly FieldInfo[] _writeFieldsReflection;
        private readonly bool _hasPrimitiveTypes;
        private readonly bool[] _primitiveType;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="jsonEventType">target type</param>
        /// <param name="service">factory for events</param>
        /// <param name="properties">written properties</param>
        /// <param name="importService">for resolving write methods</param>
        /// <throws>EventBeanManufactureException if the write method lookup fail</throws>
        public EventBeanManufacturerJsonProvided(
            JsonEventType jsonEventType,
            EventBeanTypedEventFactory service,
            WriteablePropertyDescriptor[] properties,
            ImportService importService)
        {
            _jsonEventType = jsonEventType;
            _service = service;

            _beanInstantiator = new BeanInstantiatorForgeByNewInstanceReflection(jsonEventType.UnderlyingType);

            _writeFieldsReflection = new FieldInfo[properties.Length];
            var primitiveTypeCheck = false;
            _primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                var propertyName = properties[i].PropertyName;
                var field = jsonEventType.Detail.FieldDescriptors.Get(propertyName);
                _writeFieldsReflection[i] = field.OptionalField;
                _primitiveType[i] = properties[i].PropertyType.IsPrimitive;
                primitiveTypeCheck |= _primitiveType[i];
            }

            _hasPrimitiveTypes = primitiveTypeCheck;
        }

        public EventBean Make(object[] propertyValues)
        {
            var outObject = MakeUnderlying(propertyValues);
            return _service.AdapterForTypedJson(outObject, _jsonEventType);
        }

        public object MakeUnderlying(object[] propertyValues)
        {
            var outObject = _beanInstantiator.Instantiate();

            if (!_hasPrimitiveTypes) {
                for (var i = 0; i < _writeFieldsReflection.Length; i++) {
                    try {
                        _writeFieldsReflection[i].SetValue(outObject, propertyValues[i]);
                    }
                    catch (MemberAccessException e) {
                        Handle(e, _writeFieldsReflection[i].Name);
                    }
                }
            }
            else {
                for (var i = 0; i < _writeFieldsReflection.Length; i++) {
                    if (_primitiveType[i]) {
                        if (propertyValues[i] == null) {
                            continue;
                        }
                    }

                    try {
                        _writeFieldsReflection[i].SetValue(outObject, propertyValues[i]);
                    }
                    catch (MemberAccessException e) {
                        Handle(e, _writeFieldsReflection[i].Name);
                    }
                }
            }

            return outObject;
        }

        private void Handle(
            MemberAccessException ex,
            string fieldName)
        {
            var message = "Unexpected exception encountered invoking setter for field '" +
                          fieldName +
                          "' on class '" +
                          _jsonEventType.UnderlyingType.Name +
                          "' : " +
                          ex.Message;
            Log.Error(message, ex);
        }
    }
} // end of namespace