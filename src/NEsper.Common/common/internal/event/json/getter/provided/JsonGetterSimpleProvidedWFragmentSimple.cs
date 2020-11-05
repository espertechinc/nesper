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
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.getter.provided
{
    /// <summary>
    ///     Property getter for Json underlying fields.
    /// </summary>
    public class JsonGetterSimpleProvidedWFragmentSimple : JsonGetterSimpleProvidedBase
    {
        public JsonGetterSimpleProvidedWFragmentSimple(
            FieldInfo field,
            EventType fragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory) : base(field, fragmentType, eventBeanTypedEventFactory)
        {
        }

        public override CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return UnderlyingFragmentCodegen(CastUnderlying(Field.DeclaringType, beanExpression), codegenMethodScope, codegenClassScope);
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

            return JsonFieldGetterHelperProvided.HandleJsonProvidedCreateFragmentSimple(@object, Field, FragmentType, EventBeanTypedEventFactory);
        }

        private CodegenMethod GetFragmentCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(EventType),
                EventTypeUtility.ResolveTypeCodegen(FragmentType, EPStatementInitServicesConstants.REF));
            var method = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(Field.DeclaringType, "record");
            method.Block
                .DeclareVar<object>("value", UnderlyingGetCodegen(Ref("record"), codegenMethodScope, codegenClassScope))
                .IfRefNullReturnNull("value");
            string adapterMethod;
            if (FragmentType is BeanEventType) {
                adapterMethod = "AdapterForTypedObject";
            }
            else if (FragmentType is JsonEventType) {
                adapterMethod = "AdapterForTypedJson";
            }
            else {
                throw new IllegalStateException("Unrecognized fragment event type " + FragmentType);
            }

            method.Block.MethodReturn(ExprDotMethod(factory, adapterMethod, Ref("value"), eventType));
            return method;
        }
    }
} // end of namespace