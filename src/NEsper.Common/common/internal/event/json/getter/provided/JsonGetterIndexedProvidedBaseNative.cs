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
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.getter.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    /// Property getter for Json underlying fields.
    /// </summary>
    public class JsonGetterIndexedProvidedBaseNative : BaseNativePropertyGetter,
        JsonEventPropertyGetter
    {
        private readonly FieldInfo field;
        private readonly int index;

        public JsonGetterIndexedProvidedBaseNative(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            FieldInfo field,
            int index) : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType)
        {
            this.field = field;
            this.index = index;
        }

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(field.DeclaringType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(CollectionUtil),
                "arrayValueAtIndex",
                ExprDotName(underlyingExpression, field.Name),
                Constant(index));
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(field.DeclaringType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return StaticMethod(
                typeof(CollectionUtil),
                "arrayExistsAtIndex",
                ExprDotName(underlyingExpression, field.Name),
                Constant(index));
        }

        public override bool IsExistsProperty(EventBean eventBean)
        {
            return GetJsonExists(eventBean.Underlying);
        }

        public override object Get(EventBean eventBean)
        {
            return GetJsonProp(eventBean.Underlying);
        }

        public object GetJsonProp(object @object)
        {
            var value = JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, field);
            return CollectionUtil.ArrayValueAtIndex((Array) value, index);
        }

        public bool GetJsonExists(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedIndexedPropExists(@object, field, index);
        }

        public object GetJsonFragment(object @object)
        {
            if (!IsFragmentable) {
                return null;
            }

            var value = JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, field, index);
            if (value == null) {
                return null;
            }

            return GetFragmentFromValue(value);
        }

        public override Type TargetType => field.DeclaringType;

        public override Type BeanPropType => typeof(object);
    }
} // end of namespace