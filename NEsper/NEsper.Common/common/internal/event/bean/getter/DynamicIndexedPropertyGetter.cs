///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.magic;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a dynamic indexed property (syntax field.indexed[0]?), using vanilla reflection.
    /// </summary>
    public class DynamicIndexedPropertyGetter : DynamicPropertyGetterBase
    {
        private readonly string _propertyName;
        private readonly string _getterMethodName;
        private readonly int _index;
        private readonly object[] _parameters;

        public DynamicIndexedPropertyGetter(
            string fieldName,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            _propertyName = PropertyHelper.GetPropertyName(fieldName);
            _getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            _parameters = new object[] {index};
            _index = index;
        }

        internal override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicIndexPropertyDetermineMethod(clazz, _propertyName, _getterMethodName);
        }

        internal override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(DynamicIndexedPropertyGetter),
                "DynamicIndexPropertyDetermineMethod",
                clazz,
                Constant(_propertyName),
                Constant(_getterMethodName));
        }

        internal override object Call(
            DynamicPropertyDescriptor descriptor,
            object underlying)
        {
            return DynamicIndexedPropertyGet(descriptor, underlying, _parameters, _index);
        }

        internal override CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var @params = codegenClassScope.AddDefaultFieldUnshared<object[]>(true, Constant(_parameters));
            return StaticMethod(
                typeof(DynamicIndexedPropertyGetter),
                "DynamicIndexedPropertyGet",
                desc,
                @object,
                @params,
                Constant(_index));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="propertyName">property name</param>
        /// <param name="getterMethodName">method</param>
        /// <returns>null or method</returns>
        public static MethodInfo DynamicIndexPropertyDetermineMethod(
            Type clazz,
            string propertyName,
            string getterMethodName)
        {
            MethodInfo method = null;

            try {
                method = clazz.GetMethod(getterMethodName, new[] {typeof(int)});
                if (method != null) {
                    return method;
                }
            }
            catch (AmbiguousMatchException) {
            }

            // Getting here means there is no "indexed" method matching the form GetXXX(int index);
            // this section attempts to now see if the method can be found in such a way that it
            // return an array (or presumably a list) that can be indexed.  We've added to this by
            // augmenting it with the property name.  As we know, c# properties simply mask
            // properties that have a similar form to the ones outlined herein.

            var property = clazz.GetProperty(propertyName);
            if (property != null && property.CanRead) {
                method = property.GetGetMethod();
            }

            if (method == null) {
                method = clazz.GetMethod(getterMethodName, new Type[0]);
            }

            if (method != null) {
                if (method.ReturnType.IsArray) {
                    return method;
                }

                if (method.ReturnType.IsGenericList()) {
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
        /// <param name="index">idx</param>
        /// <returns>null or method</returns>
        public static object DynamicIndexedPropertyGet(
            DynamicPropertyDescriptor descriptor,
            object underlying,
            object[] parameters,
            int index)
        {
            try {
                if (descriptor.HasParameters) {
                    return descriptor.Method.Invoke(underlying, parameters);
                }

                var result = descriptor.Method.Invoke(underlying, null);
                if (result == null) {
                    return null;
                }

                if (result is Array array) {
                    return array.Length > index ? array.GetValue(index) : null;
                }

                if (result.GetType().IsGenericList()) {
                    var list = result.AsObjectList(MagicMarker.SingletonInstance);
                    return list.Count > index ? list[index] : null;
                }

                return null;
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(descriptor.Method.Target, underlying, e);
            }
        }
    }
} // end of namespace