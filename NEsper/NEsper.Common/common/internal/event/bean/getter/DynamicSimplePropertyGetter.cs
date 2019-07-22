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
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.getter
{
    /// <summary>
    ///     Getter for a dynamic property (syntax field.inner?), using vanilla reflection.
    /// </summary>
    public class DynamicSimplePropertyGetter : DynamicPropertyGetterBase
    {
        private readonly string _getterMethodName;
        private readonly string _isMethodName;

        public DynamicSimplePropertyGetter(
            string fieldName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            _getterMethodName = PropertyHelper.GetGetterMethodName(fieldName);
            _isMethodName = PropertyHelper.GetIsMethodName(fieldName);
        }

        internal override object Call(
            DynamicPropertyDescriptor descriptor,
            object underlying)
        {
            return DynamicSimplePropertyCall(descriptor, underlying);
        }

        internal override CodegenExpression CallCodegen(
            CodegenExpressionRef desc,
            CodegenExpressionRef @object,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "dynamicSimplePropertyCall", desc, @object);
        }

        internal override MethodInfo DetermineMethod(Type clazz)
        {
            return DynamicSimplePropertyDetermineMethod(_getterMethodName, _isMethodName, clazz);
        }

        internal override CodegenExpression DetermineMethodCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                GetType(),
                "dynamicSimplePropertyDetermineMethod",
                Constant(_getterMethodName),
                Constant(_isMethodName),
                clazz);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">desc</param>
        /// <param name="underlying">underlying</param>
        /// <returns>value</returns>
        public static object DynamicSimplePropertyCall(
            DynamicPropertyDescriptor descriptor,
            object underlying)
        {
            try {
                return descriptor.Method.Invoke(underlying, null);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception e) {
                throw HandleException(descriptor, underlying, e);
            }
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="getterMethodName">getter</param>
        /// <param name="isMethodName">is-method</param>
        /// <param name="clazz">class</param>
        /// <returns>method or null</returns>
        public static MethodInfo DynamicSimplePropertyDetermineMethod(
            string getterMethodName,
            string isMethodName,
            Type clazz)
        {
            try {
                return clazz.GetMethod(getterMethodName);
            }
            catch (Exception ex1) when (ex1 is AmbiguousMatchException || ex1 is ArgumentNullException) {
                try {
                    return clazz.GetMethod(isMethodName);
                }
                catch (Exception ex2) when (ex2 is AmbiguousMatchException || ex2 is ArgumentNullException) {
                    return null;
                }
            }
        }
    }
} // end of namespace