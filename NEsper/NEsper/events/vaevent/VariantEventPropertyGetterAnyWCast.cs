///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;
using com.espertech.esper.util;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.vaevent
{
    public class VariantEventPropertyGetterAnyWCast : EventPropertyGetterSPI
    {
        private readonly int _assignedPropertyNumber;
        private readonly SimpleTypeCaster _caster;
        private readonly VariantPropertyGetterCache _propertyGetterCache;

        public VariantEventPropertyGetterAnyWCast(VariantPropertyGetterCache propertyGetterCache,
            int assignedPropertyNumber, SimpleTypeCaster caster)
        {
            _propertyGetterCache = propertyGetterCache;
            _assignedPropertyNumber = assignedPropertyNumber;
            _caster = caster;
        }

        public object Get(EventBean eventBean)
        {
            var value = VariantEventPropertyGetterAny.VariantGet(eventBean, _propertyGetterCache,
                _assignedPropertyNumber);
            if (value == null) return null;
            return _caster.Invoke(value);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return VariantEventPropertyGetterAny.VariantExists(eventBean, _propertyGetterCache,
                _assignedPropertyNumber);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null;
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return LocalMethod(GetCodegen(context), beanExpression);
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(VariantPropertyGetterCache), _propertyGetterCache);
            return StaticMethod(typeof(VariantEventPropertyGetterAny), "variantExists", beanExpression,
                Ref(member.MemberName), Constant(_assignedPropertyNumber));
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            throw VariantEventPropertyGetterAny.VariantImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VariantEventPropertyGetterAny.VariantImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VariantEventPropertyGetterAny.VariantImplementationNotProvided();
        }

        private string GetCodegen(ICodegenContext context)
        {
            var mCache = context.MakeAddMember(typeof(VariantPropertyGetterCache), _propertyGetterCache);
            var mCaster = context.MakeAddMember(typeof(SimpleTypeCaster), _caster);
            return context.AddMethod(typeof(object), typeof(EventBean), "eventBean", GetType())
                .DeclareVar(typeof(object), "value",
                    StaticMethod(typeof(VariantEventPropertyGetterAny), "variantGet",
                        Ref("eventBean"), Ref(mCache.MemberName),
                        Constant(_assignedPropertyNumber)))
                .MethodReturn(ExprDotMethod(Ref(mCaster.MemberName), "cast",
                    Ref("value")));
        }
    }
} // end of namespace