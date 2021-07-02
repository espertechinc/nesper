///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Copy method for bean events utilizing a copy mechanism encapsulated by the copier.
    /// </summary>
    public class BeanEventBeanObjectCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly BeanEventType _beanEventType;
        private readonly IObjectCopier _copier;

        public BeanEventBeanObjectCopyMethodForge(
            BeanEventType beanEventType,
            IObjectCopier copier)
        {
            _beanEventType = beanEventType;
            _copier = copier;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance<BeanEventBeanObjectCopyMethod>(
                Cast(
                    typeof(BeanEventType),
                    EventTypeUtility.ResolveTypeCodegen(_beanEventType, EPStatementInitServicesConstants.REF)),
                factory,
                ExprDotName(EPStatementInitServicesConstants.REF, "ObjectCopier"));
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new BeanEventBeanObjectCopyMethod(_beanEventType, eventBeanTypedEventFactory, _copier);
        }
    }
} // end of namespace