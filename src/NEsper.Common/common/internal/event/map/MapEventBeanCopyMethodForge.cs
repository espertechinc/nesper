///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class MapEventBeanCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly MapEventType mapEventType;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="mapEventType">map event type</param>
        /// <param name="eventBeanTypedEventFactory">for copying events</param>
        public MapEventBeanCopyMethodForge(
            MapEventType mapEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.mapEventType = mapEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance<MapEventBeanCopyMethod>(
                Cast(
                    typeof(MapEventType),
                    EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new MapEventBeanCopyMethod(mapEventType, eventBeanTypedEventFactory);
        }

        public EventBean Copy(EventBean theEvent)
        {
            var mapped = (MappedEventBean)theEvent;
            var props = mapped.Properties;
            return eventBeanTypedEventFactory.AdapterForTypedMap(new Dictionary<string, object>(props), mapEventType);
        }
    }
} // end of namespace