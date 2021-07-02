///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.@event.variant.VariantEventPropertyGetterAny;

namespace com.espertech.esper.common.@internal.@event.variant
{
    public class VariantEventPropertyGetterAnyWCast : EventPropertyGetterSPI
    {
        private readonly SimpleTypeCaster _caster;
        private readonly string _propertyName;
        private readonly VariantEventType _variantEventType;

        public VariantEventPropertyGetterAnyWCast(
            VariantEventType variantEventType,
            string propertyName,
            SimpleTypeCaster caster)
        {
            this._variantEventType = variantEventType;
            this._propertyName = propertyName;
            this._caster = caster;
        }

        public object Get(EventBean eventBean)
        {
            var value = VariantGet(eventBean, _variantEventType.VariantPropertyGetterCache, _propertyName);
            if (value == null) {
                return null;
            }

            return _caster.Cast(value);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return VariantExists(eventBean, _variantEventType.VariantPropertyGetterCache, _propertyName);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return LocalMethod(GetCodegen(codegenMethodScope, codegenClassScope), beanExpression);
        }

        public CodegenExpression EventBeanExistsCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var cache = codegenClassScope.AddOrGetDefaultFieldSharable(
                new VariantPropertyGetterCacheCodegenField(_variantEventType));
            return StaticMethod(
                typeof(VariantEventPropertyGetterAny),
                "VariantExists",
                beanExpression,
                cache,
                Constant(_propertyName));
        }

        public CodegenExpression EventBeanFragmentCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            return ConstantNull();
        }

        public CodegenExpression UnderlyingGetCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            throw VariantImplementationNotProvided();
        }

        public CodegenExpression UnderlyingExistsCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            throw VariantImplementationNotProvided();
        }

        public CodegenExpression UnderlyingFragmentCodegen(
            CodegenExpression underlyingExpression,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            throw VariantImplementationNotProvided();
        }

        private CodegenMethod GetCodegen(
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var cache = codegenClassScope.AddOrGetDefaultFieldSharable(
                new VariantPropertyGetterCacheCodegenField(_variantEventType));
            var method = codegenMethodScope.MakeChild(typeof(object), GetType(), codegenClassScope)
                .AddParam(typeof(EventBean), "eventBean");
            method.Block
                .DeclareVar<object>(
                    "value",
                    StaticMethod(
                        typeof(VariantEventPropertyGetterAny),
                        "VariantGet",
                        Ref("eventBean"),
                        cache,
                        Constant(_propertyName)))
                .MethodReturn(_caster.Codegen(Ref("value"), typeof(object), method, codegenClassScope));
            return method;
        }
    }
} // end of namespace