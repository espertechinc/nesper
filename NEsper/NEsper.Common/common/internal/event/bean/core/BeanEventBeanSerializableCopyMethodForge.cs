///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
	/// <summary>
	///     Copy method for bean events utilizing serializable.
	/// </summary>
	public class BeanEventBeanSerializableCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly IContainer _container;
        private readonly BeanEventType _beanEventType;

        public BeanEventBeanSerializableCopyMethodForge(
            IContainer container,
            BeanEventType beanEventType)
        {
            _beanEventType = beanEventType;
            _container = container;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance(
                typeof(BeanEventBeanSerializableCopyMethod),
                Cast(typeof(BeanEventType), EventTypeUtility.ResolveTypeCodegen(_beanEventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new BeanEventBeanSerializableCopyMethod(
                _container.Resolve<IObjectCopier>(),
                _beanEventType,
                eventBeanTypedEventFactory);
        }
    }
} // end of namespace