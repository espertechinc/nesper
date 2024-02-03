///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

        private readonly BeanEventType beanEventType;

        private readonly BeanInstantiator beanInstantiator;
        private readonly bool hasPrimitiveTypes;
        private readonly bool[] primitiveType;
        private readonly EventBeanTypedEventFactory service;
        private readonly MemberInfo[] writeMembersReflection;

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

            writeMembersReflection = new MemberInfo[properties.Length];
            var primitiveTypeCheck = false;
            primitiveType = new bool[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                writeMembersReflection[i] = properties[i].WriteMember;
                primitiveType[i] = properties[i].PropertyType.IsValueType;
                primitiveTypeCheck |= primitiveType[i];
            }

            hasPrimitiveTypes = primitiveTypeCheck;
        }

        public EventBean Make(object[] propertyValues)
        {
            var outObject = MakeUnderlying(propertyValues);
            return service.AdapterForTypedObject(outObject, beanEventType);
        }

        public object MakeUnderlying(object[] propertyValues)
        {
            var outObject = beanInstantiator.Instantiate();

            if (!hasPrimitiveTypes) {
                var parameters = new object[1];
                for (var i = 0; i < writeMembersReflection.Length; i++) {
                    parameters[0] = propertyValues[i];
                    try {
                        var writeMember = writeMembersReflection[i];
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
                        Handle(e, writeMembersReflection[i].Name);
                    }
                    catch (Exception e) {
                        var message = "Unexpected exception encountered invoking setter-method '" +
                                      writeMembersReflection[i] +
                                      "' on class '" +
                                      beanEventType.UnderlyingType.Name +
                                      "' : " +
                                      e.Message;
                        Log.Error(message, e);
                    }
                }
            }
            else {
                var parameters = new object[1];
                for (var i = 0; i < writeMembersReflection.Length; i++) {
                    if (primitiveType[i] && propertyValues[i] == null) {
                        continue;
                    }

                    parameters[0] = propertyValues[i];
                    try {
                        var writeMember = writeMembersReflection[i];
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
                        Handle(e, writeMembersReflection[i].Name);
                    }
                    catch (Exception e) {
                        HandleAny(e, writeMembersReflection[i].Name);
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
                          beanEventType.UnderlyingType.Name +
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
                          beanEventType.UnderlyingType.Name +
                          "' : " +
                          ex.Message;
            Log.Error(message, ex);
        }
    }
} // end of namespace