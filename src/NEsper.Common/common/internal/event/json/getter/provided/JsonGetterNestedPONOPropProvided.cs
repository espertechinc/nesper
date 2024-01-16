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
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.getter.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static
    com.espertech.esper.common.@internal.@event.json.getter.provided.
    JsonFieldGetterHelperProvided; // getJsonProvidedSimpleProp

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    public class JsonGetterNestedPONOPropProvided : BaseNativePropertyGetter,
        JsonEventPropertyGetter
    {
        private readonly FieldInfo _field;
        private readonly BeanEventPropertyGetter _nestedGetter;

        public JsonGetterNestedPONOPropProvided(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            Type genericType,
            FieldInfo field,
            BeanEventPropertyGetter nestedGetter) : base(
            eventBeanTypedEventFactory,
            beanEventTypeFactory,
            returnType)
        {
            this._field = field;
            this._nestedGetter = nestedGetter;
        }

        public override Type TargetType => _field.DeclaringType;

        //public override Type BeanPropType => typeof(object);

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(
                CastUnderlying(_field.DeclaringType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFieldCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public override CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingExistsCodegen(
                CastUnderlying(_field.DeclaringType, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetFieldExistsCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
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
            var nested = GetJsonProvidedSimpleProp(@object, _field);
            if (nested == null) {
                return null;
            }

            return _nestedGetter.GetBeanProp(nested);
        }

        public bool GetJsonExists(object @object)
        {
            var nested = GetJsonProvidedSimpleProp(@object, _field);
            if (nested == null) {
                return false;
            }

            return _nestedGetter.IsBeanExistsProperty(nested);
        }

        public object GetJsonFragment(object @object)
        {
            return null;
        }

        private CodegenMethod GetFieldCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var propertyType = _nestedGetter.BeanPropType.GetBoxedType();
            return codegenMethodScope.MakeChild(propertyType, GetType(), codegenClassScope)
                .AddParam(_field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", Ref("und." + _field.Name))
                .IfRefNullReturnNull("value")
                .MethodReturn(
                    _nestedGetter.UnderlyingGetCodegen(
                        CastRef(_nestedGetter.TargetType, "value"),
                        codegenMethodScope,
                        codegenClassScope));
        }

        private CodegenMethod GetFieldExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(_field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", Ref("und." + _field.Name))
                .IfRefNullReturnFalse("value")
                .MethodReturn(
                    _nestedGetter.UnderlyingExistsCodegen(
                        CastRef(_nestedGetter.TargetType, "value"),
                        codegenMethodScope,
                        codegenClassScope));
        }
    }
} // end of namespace