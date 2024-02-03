///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.json.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.fromschema
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public class JsonGetterSimpleSchemaWFragmentArray : JsonGetterSimpleSchemaBase
    {
        public JsonGetterSimpleSchemaWFragmentArray(
            JsonUnderlyingField field,
            string underlyingClassName,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory) : base(
            field,
            underlyingClassName,
            fragmentType,
            eventBeanTypedEventFactory)
        {
        }

        public override CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(
                CastUnderlying(UnderlyingClassName, beanExpression),
                codegenMethodScope,
                codegenClassScope);
        }

        public override CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            if (FragmentType == null) {
                return ConstantNull();
            }

            CodegenExpression factory =
                codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            CodegenExpression eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(FragmentType, EPStatementInitServicesConstants.REF));
            return StaticMethod(
                typeof(JsonFieldGetterHelperSchema),
                "HandleJsonCreateFragmentArray",
                underlyingExpression,
                Constant(Field.PropertyNumber),
                eventType,
                factory);
        }

        public override object GetJsonFragment(object @object)
        {
            if (FragmentType == null) {
                return null;
            }

            return JsonFieldGetterHelperSchema.HandleJsonCreateFragmentArray(
                (JsonEventObjectBase)@object,
                Field.PropertyNumber,
                FragmentType,
                EventBeanTypedEventFactory);
        }
    }
} // end of namespace