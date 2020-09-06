///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.getter.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    public sealed class JsonGetterIndexedSchema : JsonGetterIndexedBase
    {
        private readonly JsonUnderlyingField _field;

        public JsonGetterIndexedSchema(
            int index,
            string underlyingClassName,
            EventType optionalInnerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            JsonUnderlyingField field)
            : base(index, underlyingClassName, optionalInnerType, eventBeanTypedEventFactory)
        {
            this._field = field;
        }

        public override string FieldName => _field.FieldName;

        public override CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (OptionalInnerType == null) {
                return ConstantNull();
            }

            CodegenExpression factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            CodegenExpression eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(OptionalInnerType, EPStatementInitServicesConstants.REF));
            return StaticMethod(
                typeof(JsonFieldGetterHelperSchema),
                "HandleJsonCreateFragmentIndexed",
                underlyingExpression,
                Constant(_field.FieldName),
                Constant(Index),
                eventType,
                factory);
        }

        public override object GetJsonProp(object @object)
        {
            return JsonFieldGetterHelperSchema.GetJsonIndexedProp(@object, _field.FieldName, Index);
        }

        public override bool GetJsonExists(object @object)
        {
            return JsonFieldGetterHelperSchema.GetJsonIndexedPropExists(@object, _field, Index);
        }

        public override object GetJsonFragment(object @object)
        {
            if (OptionalInnerType == null) {
                return null;
            }

            var value = JsonFieldGetterHelperSchema.GetJsonIndexedProp(@object, _field.FieldName, Index);
            if (value == null) {
                return null;
            }

            return EventBeanTypedEventFactory.AdapterForTypedJson(value, OptionalInnerType);
        }
    }
} // end of namespace