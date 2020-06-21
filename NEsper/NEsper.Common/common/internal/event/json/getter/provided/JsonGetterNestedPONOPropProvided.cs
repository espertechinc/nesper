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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.json.getter.provided.JsonFieldGetterHelperProvided; // getJsonProvidedSimpleProp

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    public class JsonGetterNestedPONOPropProvided : BaseNativePropertyGetter,
        JsonEventPropertyGetter
    {
        private readonly FieldInfo field;
        private readonly BeanEventPropertyGetter nestedGetter;

        public JsonGetterNestedPONOPropProvided(
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType,
            Type genericType,
            FieldInfo field,
            BeanEventPropertyGetter nestedGetter) : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType, genericType)
        {
            this.field = field;
            this.nestedGetter = nestedGetter;
        }

        public override Type TargetType => field.DeclaringType;

        public override Type BeanPropType => typeof(object);

        public override CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingGetCodegen(CastUnderlying(field.DeclaringType, beanExpression), codegenMethodScope, codegenClassScope);
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
            return UnderlyingExistsCodegen(CastUnderlying(field.DeclaringType, beanExpression), codegenMethodScope, codegenClassScope);
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
            var nested = GetJsonProvidedSimpleProp(@object, field);
            if (nested == null) {
                return null;
            }

            return nestedGetter.GetBeanProp(nested);
        }

        public bool GetJsonExists(object @object)
        {
            var nested = GetJsonProvidedSimpleProp(@object, field);
            if (nested == null) {
                return false;
            }

            return nestedGetter.IsBeanExistsProperty(nested);
        }

        public object GetJsonFragment(object @object)
        {
            return null;
        }

        private CodegenMethod GetFieldCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", Ref("und." + field.Name))
                .IfRefNullReturnNull("value")
                .MethodReturn(nestedGetter.UnderlyingGetCodegen(CastRef(nestedGetter.TargetType, "value"), codegenMethodScope, codegenClassScope));
        }

        private CodegenMethod GetFieldExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", Ref("und." + field.Name))
                .IfRefNullReturnFalse("value")
                .MethodReturn(nestedGetter.UnderlyingExistsCodegen(CastRef(nestedGetter.TargetType, "value"), codegenMethodScope, codegenClassScope));
        }
    }
} // end of namespace