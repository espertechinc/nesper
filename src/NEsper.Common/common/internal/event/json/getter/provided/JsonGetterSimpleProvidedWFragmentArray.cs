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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public class JsonGetterSimpleProvidedWFragmentArray : JsonGetterSimpleProvidedBase
    {
        public JsonGetterSimpleProvidedWFragmentArray(
            FieldInfo field,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory) : base(
            field,
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
                CastUnderlying(Field.DeclaringType, beanExpression),
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

            return LocalMethod(GetFragmentCodegen(codegenMethodScope, codegenClassScope), underlyingExpression);
        }

        public override object GetJsonFragment(object @object)
        {
            if (FragmentType == null) {
                return null;
            }

            var value = JsonFieldGetterHelperProvided.GetJsonProvidedSimpleProp(@object, Field);
            return JsonFieldGetterHelperProvided.HandleJsonProvidedCreateFragmentArray(
                value,
                FragmentType,
                EventBeanTypedEventFactory);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory =
                codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(FragmentType, EPStatementInitServicesConstants.REF));
            return codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(Field.DeclaringType, "record")
                .Block
                .DeclareVar<object>("value", UnderlyingGetCodegen(Ref("record"), codegenMethodScope, codegenClassScope))
                .MethodReturn(
                    StaticMethod(
                        typeof(JsonFieldGetterHelperProvided),
                        "HandleJsonProvidedCreateFragmentArray",
                        Ref("value"),
                        eventType,
                        factory));
        }
    }
} // end of namespace