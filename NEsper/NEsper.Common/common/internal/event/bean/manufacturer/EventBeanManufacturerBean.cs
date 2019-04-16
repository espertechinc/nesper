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
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.manufacturer
{
    /// <summary>
    ///     Factory for event beans created and populate anew from a set of values.
    /// </summary>
    public class EventBeanManufacturerBean : EventBeanManufacturer
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventType beanEventType;

        private readonly BeanInstantiator beanInstantiator;
        private readonly bool hasPrimitiveTypes;
        private readonly bool[] primitiveType;
        private readonly EventBeanTypedEventFactory service;
        private readonly MethodInfo[] writeMethodsReflection;

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
            this.beanEventType = beanEventType;
            this.service = service;

            beanInstantiator = BeanInstantiatorFactory.MakeInstantiator(beanEventType, importService)
                .BeanInstantiator;

            writeMethodsReflection = new MethodInfo[properties.Length];
            var primitiveTypeCheck = false;
            primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writeMethodsReflection[i] = properties[i].WriteMethod;
                primitiveType[i] = properties[i].PropertyType.IsPrimitive;
                primitiveTypeCheck |= primitiveType[i];
            }

            hasPrimitiveTypes = primitiveTypeCheck;
        }

        public EventBean Make(object[] propertyValues)
        {
            var outObject = MakeUnderlying(propertyValues);
            return service.AdapterForTypedBean(outObject, beanEventType);
        }

        public object MakeUnderlying(object[] propertyValues)
        {
            var outObject = beanInstantiator.Instantiate();

            if (!hasPrimitiveTypes) {
                var parameters = new object[1];
                for (var i = 0; i < writeMethodsReflection.Length; i++) {
                    parameters[0] = propertyValues[i];
                    try {
                        writeMethodsReflection[i].Invoke(outObject, parameters);
                    }
                    catch (MemberAccessException e) {
                        Handle(e, writeMethodsReflection[i].Name);
                    }
                    catch (Exception e) {
                        var message = "Unexpected exception encountered invoking setter-method '" +
                                      writeMethodsReflection[i] + "' on class '" +
                                      beanEventType.UnderlyingType.Name + "' : " + e.Message;
                        Log.Error(message, e);
                    }
                }
            }
            else {
                var parameters = new object[1];
                for (var i = 0; i < writeMethodsReflection.Length; i++) {
                    if (primitiveType[i]) {
                        if (propertyValues[i] == null) {
                            continue;
                        }
                    }

                    parameters[0] = propertyValues[i];
                    try {
                        writeMethodsReflection[i].Invoke(outObject, parameters);
                    }
                    catch (MemberAccessException e) {
                        Handle(e, writeMethodsReflection[i].Name);
                    }
                    catch (Exception e) {
                        HandleAny(e, writeMethodsReflection[i].Name);
                    }
                }
            }

            return outObject;
        }

        private void HandleAny(
            Exception ex,
            string methodName)
        {
            var message = "Unexpected exception encountered invoking setter-method '" + methodName + "' on class '" +
                          beanEventType.UnderlyingType.Name + "' : " + ex.Message;
            Log.Error(message, ex);
        }

        private void Handle(
            MemberAccessException ex,
            string methodName)
        {
            var message = "Unexpected exception encountered invoking setter-method '" + methodName + "' on class '" +
                          beanEventType.UnderlyingType.Name + "' : " + ex.Message;
            Log.Error(message, ex);
        }
    }
} // end of namespace