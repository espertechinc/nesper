///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.getter.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public sealed class JsonGetterIndexedProvided : JsonGetterIndexedBase
    {
        private readonly FieldInfo _field;

        public JsonGetterIndexedProvided(
            int index,
            string underlyingClassName,
            EventType optionalInnerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            FieldInfo field)
            : base(index, underlyingClassName, optionalInnerType, eventBeanTypedEventFactory)
        {
            _field = field;
        }

        public override string FieldName => _field.Name;

        public override CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (OptionalInnerType == null) {
                return ConstantNull();
            }

            CodegenExpression factory =
                codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            CodegenExpression eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(OptionalInnerType, EPStatementInitServicesConstants.REF));
            return StaticMethod(
                typeof(JsonFieldGetterHelperProvided),
                "HandleJsonProvidedCreateFragmentIndexed",
                ExprDotName(underlyingExpression, _field.Name),
                Constant(Index),
                eventType,
                factory);
        }

        public override object GetJsonProp(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, _field, Index);
        }

        public override bool GetJsonExists(object @object)
        {
            return JsonFieldGetterHelperProvided.GetJsonProvidedIndexedPropExists(@object, _field, Index);
        }

        public override object GetJsonFragment(object @object)
        {
            if (OptionalInnerType == null) {
                return null;
            }

            var value = JsonFieldGetterHelperProvided.GetJsonProvidedIndexedProp(@object, _field, Index);
            if (value == null) {
                return null;
            }

            return EventBeanTypedEventFactory.AdapterForTypedJson(value, OptionalInnerType);
        }
    }
} // end of namespace