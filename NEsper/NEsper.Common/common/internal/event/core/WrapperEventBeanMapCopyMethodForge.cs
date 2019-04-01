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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.core
{
    /// <summary>
    ///     Copy method for wrapper events.
    /// </summary>
    public class WrapperEventBeanMapCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly WrapperEventType wrapperEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="wrapperEventType">wrapper type</param>
        public WrapperEventBeanMapCopyMethodForge(WrapperEventType wrapperEventType)
        {
            this.wrapperEventType = wrapperEventType;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance(
                typeof(WrapperEventBeanMapCopyMethod),
                Cast(
                    typeof(WrapperEventType),
                    EventTypeUtility.ResolveTypeCodegen(wrapperEventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new WrapperEventBeanMapCopyMethod(wrapperEventType, eventBeanTypedEventFactory);
        }
    }
} // end of namespace