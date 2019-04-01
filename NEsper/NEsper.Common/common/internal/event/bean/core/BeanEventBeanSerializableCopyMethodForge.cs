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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    /// Copy method for bean events utilizing serializable.
    /// </summary>
    public class BeanEventBeanSerializableCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly BeanEventType beanEventType;

        public BeanEventBeanSerializableCopyMethodForge(BeanEventType beanEventType)
        {
            this.beanEventType = beanEventType;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            CodegenExpressionField factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance(typeof(BeanEventBeanSerializableCopyMethod),
                    Cast(typeof(BeanEventType), EventTypeUtility.ResolveTypeCodegen(beanEventType, EPStatementInitServicesConstants.REF)),
                    factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new BeanEventBeanSerializableCopyMethod(beanEventType, eventBeanTypedEventFactory);
        }
    }
} // end of namespace