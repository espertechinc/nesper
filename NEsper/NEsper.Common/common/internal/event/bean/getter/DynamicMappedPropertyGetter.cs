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

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a dynamic mapped property (syntax field.mapped('key')?), using vanilla reflection.
    /// </summary>
    public class DynamicMappedPropertyGetter : DynamicPropertyGetterBase
    {
        private readonly string _propertyName;
        private readonly string _getterMethodName;
        private readonly object[] _parameters;

        public DynamicMappedPropertyGetter(
            string fieldName,
            string key,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            _propertyName = PropertyHelper.GetPropertyName(fieldName);
            _getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            _parameters = new object[] {key};
        }

        internal override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicMapperPropertyDetermineMethod(clazz, _propertyName, _getterMethodName);
        }

        internal override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(DynamicMappedPropertyGetter),
                "DynamicMapperPropertyDetermineMethod",
                clazz,
                Constant(_propertyName),
                Constant(_getterMethodName));
        }

        internal override object Call(
            DynamicPropertyDescriptor descriptor,
            object underlying)
        {
            return DynamicMappedPropertyGet(descriptor, underlying, _parameters);
        }

        internal override CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var @params = codegenClassScope.AddDefaultFieldUnshared<object[]>(true, Constant(_parameters));
            return StaticMethod(
                typeof(DynamicMappedPropertyGetter),
                "DynamicMappedPropertyGet",
                desc,
                @object,
                @params);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="getterMethodName">method</param>
        /// <returns>value</returns>
        /// <throws>PropertyAccessException for access ex</throws>
        public static MethodInfo DynamicMapperPropertyDetermineMethod(
            Type clazz,
            string propertyName,
            string getterMethodName)
        {
            MethodInfo method = null;

            try
            {
                method = clazz.GetMethod(getterMethodName, new[] { typeof(string) });
                if (method != null)
                {
                    return method;
                }
            }
            catch (AmbiguousMatchException)
            {
            }

            // Getting here means there is no "indexed" method matching the form GetXXX(int index);
            // this section attempts to now see if the method can be found in such a way that it
            // return an array (or presumably a list) that can be indexed.  We've added to this by
            // augmenting it with the property name.  As we know, c# properties simply mask
            // properties that have a similar form to the ones outlined herein.

            var property = clazz.GetProperty(propertyName);
            if (property != null && property.CanRead)
            {
                method = property.GetGetMethod();
            }

            if (method == null)
            {
                method = clazz.GetMethod(getterMethodName, new Type[0]);
            }

            if (method != null)
            {
                if (method.ReturnType.IsGenericStringDictionary())
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">descriptor</param>
        /// <param name="underlying">target</param>
        /// <param name="parameters">params</param>
        /// <returns>value</returns>
        public static object DynamicMappedPropertyGet(
            DynamicPropertyDescriptor descriptor,
            object underlying,
            object[] parameters)
        {
            try {
                if (descriptor.HasParameters) {
                    return descriptor.Method.Invoke(underlying, parameters);
                }

                var result = descriptor.Method.Invoke(underlying, null);
                if (result == null)
                {
                    return null;
                }

                if (result.GetType().IsGenericDictionary())
                {
                    var map = result.AsObjectDictionary(MagicMarker.SingletonInstance);
                    return map.Get(parameters[0]);
                }

                return null;
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