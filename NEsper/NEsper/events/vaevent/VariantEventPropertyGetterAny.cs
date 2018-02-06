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
using com.espertech.esper.compat;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.vaevent
{
    public class VariantEventPropertyGetterAny : EventPropertyGetterSPI
    {
        private readonly int _assignedPropertyNumber;
        private readonly VariantPropertyGetterCache _propertyGetterCache;

        public VariantEventPropertyGetterAny(VariantPropertyGetterCache propertyGetterCache, int assignedPropertyNumber)
        {
            _propertyGetterCache = propertyGetterCache;
            _assignedPropertyNumber = assignedPropertyNumber;
        }

        public object Get(EventBean eventBean)
        {
            return VariantGet(eventBean, _propertyGetterCache, _assignedPropertyNumber);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return VariantExists(eventBean, _propertyGetterCache, _assignedPropertyNumber);
        }

        public object GetFragment(EventBean eventBean)
        {
            return null; // no fragments provided as the type is not known in advance
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(VariantPropertyGetterCache), _propertyGetterCache);
            return StaticMethod(GetType(), "variantGet", beanExpression,
                Ref(member.MemberName), Constant(_assignedPropertyNumber));
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(VariantPropertyGetterCache), _propertyGetterCache);
            return StaticMethod(GetType(), "variantExists", beanExpression,
                Ref(member.MemberName), Constant(_assignedPropertyNumber));
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            throw VariantImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VariantImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventBean">bean</param>
        /// <param name="propertyGetterCache">cache</param>
        /// <param name="assignedPropertyNumber">num</param>
        /// <exception cref="PropertyAccessException">ex</exception>
        /// <returns>value</returns>
        public static object VariantGet(EventBean eventBean, VariantPropertyGetterCache propertyGetterCache,
            int assignedPropertyNumber)
        {
            var variant = (VariantEvent) eventBean;
            var getter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
            if (getter == null) return null;
            return getter.Get(variant.UnderlyingEventBean);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventBean">bean</param>
        /// <param name="propertyGetterCache">cache</param>
        /// <param name="assignedPropertyNumber">num</param>
        /// <returns>value</returns>
        public static bool VariantExists(EventBean eventBean, VariantPropertyGetterCache propertyGetterCache,
            int assignedPropertyNumber)
        {
            var variant = (VariantEvent) eventBean;
            var getter = propertyGetterCache.GetGetter(assignedPropertyNumber, variant.UnderlyingEventBean.EventType);
            return getter != null && getter.IsExistsProperty(variant.UnderlyingEventBean);
        }

        internal static UnsupportedOperationException VariantImplementationNotProvided()
        {
            return new UnsupportedOperationException(
                "Variant event type does not provide an implementation for underlying get");
        }
    }
} // end of namespace