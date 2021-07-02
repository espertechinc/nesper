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

namespace com.espertech.esper.common.@internal.@event.arr
{
    /// <summary>
    ///     Copy method for Object array-underlying events.
    /// </summary>
    public class ObjectArrayEventBeanCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly ObjectArrayEventType _objectArrayEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="objectArrayEventType">map event type</param>
        /// <param name="eventBeanTypedEventFactory">for copying events</param>
        public ObjectArrayEventBeanCopyMethodForge(
            ObjectArrayEventType objectArrayEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this._objectArrayEventType = objectArrayEventType;
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance<ObjectArrayEventBeanCopyMethod>(
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(_objectArrayEventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new ObjectArrayEventBeanCopyMethod(_objectArrayEventType, eventBeanTypedEventFactory);
        }
    }
} // end of namespace