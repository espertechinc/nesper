///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Copies an event for modification.
    /// </summary>
    public class BeanEventBeanConfiguredCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly BeanEventType _beanEventType;
        private readonly MethodInfo _copyMethod;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="beanEventType">type of bean to copy</param>
        /// <param name="copyMethod">method to copy the event</param>
        public BeanEventBeanConfiguredCopyMethodForge(
            BeanEventType beanEventType,
            MethodInfo copyMethod)
        {
            _beanEventType = beanEventType;
            _copyMethod = copyMethod;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance<BeanEventBeanConfiguredCopyMethod>(
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(_beanEventType, EPStatementInitServicesConstants.REF)),
                factory,
                MethodResolver.ResolveMethodCodegenExactNonStatic(_copyMethod));
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new BeanEventBeanConfiguredCopyMethod(_beanEventType, eventBeanTypedEventFactory, _copyMethod);
        }
    }
} // end of namespace