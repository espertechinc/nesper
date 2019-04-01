///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    /// Getter for a dynamic mapped property (syntax field.mapped('key')?), using vanilla reflection.
    /// </summary>
    public class DynamicMappedPropertyGetter : DynamicPropertyGetterBase
    {
        private readonly string getterMethodName;
        private readonly object[] parameters;

        public DynamicMappedPropertyGetter(
            string fieldName, string key, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory) : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            this.parameters = new object[] {key};
        }

        internal override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicMapperPropertyDetermineMethod(clazz, getterMethodName);
        }

        internal override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz, CodegenMethodScope parent, CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(DynamicMappedPropertyGetter), "DynamicMapperPropertyDetermineMethod", clazz,
                Constant(getterMethodName));
        }

        internal override object Call(DynamicPropertyDescriptor descriptor, object underlying)
        {
            return DynamicMappedPropertyGet(descriptor, underlying, parameters);
        }

        internal override CodegenExpression CallCodegen(
            CodegenExpressionRef desc, CodegenExpressionRef @object, CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField @params = codegenClassScope.AddFieldUnshared<object[]>(true, Constant(parameters));
            return StaticMethod(
                typeof(DynamicMappedPropertyGetter), "dynamicMappedPropertyGet", desc, @object, @params);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="getterMethodName">method</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException for access ex</throws>
        public static MethodInfo DynamicMapperPropertyDetermineMethod(Type clazz, string getterMethodName)
        {
            try {
                return clazz.GetMethod(getterMethodName, new Type[] { typeof(string) });
            }
            catch (AmbiguousMatchException ex1) {
                MethodInfo method;
                try {
                    method = clazz.GetMethod(getterMethodName);
                }
                catch (AmbiguousMatchException e) {
                    return null;
                }

                if (method.ReturnType != typeof(IDictionary<string, object>)) {
                    return null;
                }

                return method;
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">descriptor</param>
        /// <param name="underlying">target</param>
        /// <param name="parameters">params</param>
        /// <returns>value</returns>
        public static object DynamicMappedPropertyGet(
            DynamicPropertyDescriptor descriptor, object underlying, object[] parameters)
        {
            try {
                if (descriptor.HasParameters) {
                    return descriptor.Method.Invoke(underlying, parameters);
                }
                else {
                    var result = descriptor.Method.Invoke(underlying, null);
                    if ((result is IDictionary<object, object> map) && (result != null)) {
                        return map.Get(parameters[0]);
                    }

                    return null;
                }
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(descriptor.Method.Target, underlying, e);
            }
            catch (TargetException e) {
                throw PropertyUtility.GetTargetException(descriptor.Method.Target, e);
            }
            catch (ArgumentException e) {
                throw PropertyUtility.GetArgumentException(descriptor.Method.Target, e);
            }
            catch (MethodAccessException e) {
                throw PropertyUtility.GetMemberAccessException(descriptor.Method.Target, e);
            }
        }
    }
} // end of namespace