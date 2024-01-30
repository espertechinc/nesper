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
using static
    com.espertech.esper.common.@internal.@event.json.getter.provided.
    JsonFieldGetterHelperProvided; // getJsonProvidedIndexedProp

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     A getter that works on PONO events residing within a Map as an event property.
    /// </summary>
    public class JsonGetterIndexedEntryPONOProvided : BaseNativePropertyGetter,
        JsonEventPropertyGetter
    {
        private readonly FieldInfo _field;
        private readonly int _index;
        private readonly BeanEventPropertyGetter _nestedGetter;

        public JsonGetterIndexedEntryPONOProvided(
            FieldInfo field,
            int index,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type returnType)
            : base(eventBeanTypedEventFactory, beanEventTypeFactory, returnType)
        {
            _field = field;
            _index = index;
            _nestedGetter = nestedGetter;
        }

        public override Type TargetType => _field.DeclaringType;

        // public override Type BeanPropType => typeof(object);

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
            var result = GetJsonProvidedIndexedProp(@object, _field, _index);
            if (result == null) {
                return null;
            }

            return _nestedGetter.GetBeanProp(result);
        }

        public bool GetJsonExists(object @object)
        {
            var result = GetJsonProvidedIndexedProp(@object, _field, _index);
            if (result == null) {
                return false;
            }

            return _nestedGetter.IsBeanExistsProperty(result);
        }

        public object GetJsonFragment(object @object)
        {
            var result = GetJsonProvidedIndexedProp(@object, _field, _index);
            if (result == null) {
                return null;
            }

            return GetFragmentFromValue(result);
        }

        private CodegenMethod GetFieldCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var propertyType = _nestedGetter.BeanPropType;
            return codegenMethodScope.MakeChild(propertyType, GetType(), codegenClassScope)
                .AddParam(_field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", ExprDotName(Ref("und"), _field.Name))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetBeanArrayValueCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            _nestedGetter,
                            _index),
                        Ref("value")));
        }

        private CodegenMethod GetFieldExistsCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return codegenMethodScope.MakeChild(typeof(bool), GetType(), codegenClassScope)
                .AddParam(_field.DeclaringType, "und")
                .Block
                .DeclareVar<object>("value", ExprDotName(Ref("und"), _field.Name))
                .MethodReturn(
                    LocalMethod(
                        BaseNestableEventUtil.GetBeanArrayValueExistsCodegen(
                            codegenMethodScope,
                            codegenClassScope,
                            _nestedGetter,
                            _index),
                        Ref("value")));
        }
    }
} // end of namespace