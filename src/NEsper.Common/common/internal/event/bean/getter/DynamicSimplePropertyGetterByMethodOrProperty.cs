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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    /// Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class DynamicSimplePropertyGetterByMethodOrProperty : DynamicPropertyGetterByMethodOrPropertyBase
    {
        private readonly string _propertyName;
        private readonly string _getterMethodName;
        private readonly string _isMethodName;

        public DynamicSimplePropertyGetterByMethodOrProperty(
            string fieldName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            _propertyName = PropertyHelper.GetPropertyName(fieldName);
            _getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            _isMethodName = PropertyHelper.GetIsMethodName(fieldName);
        }

        protected override object Call(
            DynamicPropertyDescriptorByMethod descriptor,
            object underlying)
        {
            return DynamicSimplePropertyCall(descriptor, underlying);
        }

        protected override CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "DynamicSimplePropertyCall", desc, @object);
        }

        protected override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicSimplePropertyDetermineMethod(
                _propertyName,
                _getterMethodName,
                _isMethodName,
                clazz);
        }

        protected override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "DynamicSimplePropertyDetermineMethod",
                Constant(_propertyName),
                Constant(_getterMethodName),
                Constant(_isMethodName),
                clazz);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return CacheAndExists(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CacheAndExistsCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">desc</param>
        /// <param name="underlying">underlying</param>
        /// <returns>value</returns>
        public static object DynamicSimplePropertyCall(
            DynamicPropertyDescriptorByMethod descriptor,
            object underlying)
        {
            try {
                return descriptor.Method.Invoke(underlying, null);
            }
            catch (Exception ex) {
                throw HandleException(descriptor, underlying, ex);
            }
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="getterMethodName">getter</param>
        /// <param name="isMethodName">is-method</param>
        /// <param name="clazz">class</param>
        /// <returns>method or null</returns>
        public static MethodInfo DynamicSimplePropertyDetermineMethod(
            string propertyName,
            string getterMethodName,
            string isMethodName,
            Type clazz)
        {
            var propertyInfo = clazz.GetProperty(propertyName);
            if (propertyInfo != null && propertyInfo.CanRead) {
                return propertyInfo.GetMethod;
            }

            try {
                var trueGetMethod = clazz.GetMethod(getterMethodName);
                if (trueGetMethod != null) {
                    return trueGetMethod;
                }

                // did not find a method matching the getterMethodName
            }
            catch (Exception ex1) when (ex1 is AmbiguousMatchException || ex1 is ArgumentNullException) {
                // fall through
            }

            try {
                return clazz.GetMethod(isMethodName);
            }
            catch (Exception ex2) when (ex2 is AmbiguousMatchException || ex2 is ArgumentNullException) {
                return null;
            }
        }
    }
} // end of namespace