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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a dynamic indexed property (syntax field.indexed[0]?), using vanilla reflection.
    /// </summary>
    public class DynamicIndexedPropertyGetter : DynamicPropertyGetterBase
    {
        private readonly string getterMethodName;
        private readonly int index;
        private readonly object[] parameters;

        public DynamicIndexedPropertyGetter(
            string fieldName,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            parameters = new object[] {index};
            this.index = index;
        }

        internal override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicIndexPropertyDetermineMethod(clazz, getterMethodName);
        }

        internal override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(DynamicIndexedPropertyGetter), "dynamicIndexPropertyDetermineMethod", clazz,
                Constant(getterMethodName));
        }

        internal override object Call(
            DynamicPropertyDescriptor descriptor,
            object underlying)
        {
            return DynamicIndexedPropertyGet(descriptor, underlying, parameters, index);
        }

        internal override CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            var @params = codegenClassScope.AddFieldUnshared<object[]>(true, Constant(parameters));
            return StaticMethod(
                typeof(DynamicIndexedPropertyGetter), "dynamicIndexedPropertyGet", desc, @object, @params,
                Constant(index));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="clazz">class</param>
        /// <param name="getterMethodName">method</param>
        /// <returns>null or method</returns>
        public static MethodInfo DynamicIndexPropertyDetermineMethod(
            Type clazz,
            string getterMethodName)
        {
            MethodInfo method;

            try {
                return clazz.GetMethod(getterMethodName, new Type[] { typeof(int) });
            }
            catch (Exception ex1) when (ex1 is AmbiguousMatchException || ex1 is ArgumentNullException) {
                try {
                    method = clazz.GetMethod(getterMethodName);
                }
                catch (Exception e) when (e is AmbiguousMatchException || e is ArgumentNullException) {
                    return null;
                }

                if (!method.ReturnType.IsArray) {
                    return null;
                }

                return method;
            }
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

                var array = descriptor.Method.Invoke(underlying, null) as Array;
                if (array == null) {
                    return null;
                }

                if (array.Length <= index) {
                    return null;
                }

                return array.GetValue(index);
            }
            catch (InvalidCastException e) {
                throw PropertyUtility.GetMismatchException(descriptor.Method.Target, underlying, e);
            }
        }
    }
} // end of namespace