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
    public class DynamicSimplePropertyGetterByField : DynamicPropertyGetterByFieldBase
    {
        private readonly string _fieldName;

        public DynamicSimplePropertyGetterByField(
            string fieldName,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory)
        {
            _fieldName = fieldName;
        }

        protected override FieldInfo DetermineField(Type clazz)
        {
            return DynamicSimplePropertyDetermineField(_fieldName, clazz);
        }

        protected override CodegenExpression DetermineFieldCodegen(
            CodegenExpressionRef clazz,
            CodegenMethodScope parent,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(GetType(), "DynamicSimplePropertyDetermineField", Constant(_fieldName), clazz);
        }

        protected override object Call(
            DynamicPropertyDescriptorByField descriptor,
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

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return CacheAndExistsCodegen(underlyingExpression, codegenMethodScope, codegenClassScope);
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return CacheAndExists(Cache, this, eventBean.Underlying, EventBeanTypedEventFactory);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="fieldName">name</param>
        /// <param name="clazz">class</param>
        /// <returns>method or null</returns>
        public static FieldInfo DynamicSimplePropertyDetermineField(
            string fieldName,
            Type clazz)
        {
            return clazz.GetField(fieldName);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="descriptor">desc</param>
        /// <param name="underlying">underlying</param>
        /// <returns>value</returns>
        public static object DynamicSimplePropertyCall(
            DynamicPropertyDescriptorByField descriptor,
            object underlying)
        {
            try {
                return descriptor.Field.GetValue(underlying);
            }
            catch (Exception ex) {
                throw HandleException(descriptor, underlying, ex);
            }
        }
    }
} // end of namespace