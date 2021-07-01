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

namespace NEsper.Avro.Core
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class AvroEventBeanCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly AvroEventType _avroEventType;

        public AvroEventBeanCopyMethodForge(AvroEventType avroEventType)
        {
            _avroEventType = avroEventType;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return CodegenExpressionBuilder.NewInstance(
                typeof(AvroEventBeanCopyMethod),
                CodegenExpressionBuilder.Cast(
                    typeof(AvroEventType),
                    EventTypeUtility.ResolveTypeCodegen(_avroEventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new AvroEventBeanCopyMethod(_avroEventType, eventBeanTypedEventFactory);
        }
    }
} // end of namespace