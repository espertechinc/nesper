///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
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
    public class MapEventBeanCopyMethodWithArrayMapForge : EventBeanCopyMethodForge
    {
        private readonly string[] arrayPropertiesToCopy;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly MapEventType mapEventType;
        private readonly string[] mapPropertiesToCopy;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="mapEventType">map event type</param>
        /// <param name="eventBeanTypedEventFactory">for copying events</param>
        /// <param name="mapPropertiesToCopy">map props</param>
        /// <param name="arrayPropertiesToCopy">array props</param>
        public MapEventBeanCopyMethodWithArrayMapForge(
            MapEventType mapEventType, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ISet<string> mapPropertiesToCopy, ISet<string> arrayPropertiesToCopy)
        {
            this.mapEventType = mapEventType;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.mapPropertiesToCopy = mapPropertiesToCopy.ToArray();
            this.arrayPropertiesToCopy = arrayPropertiesToCopy.ToArray();
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance(
                typeof(MapEventBeanCopyMethodWithArrayMap),
                Cast(
                    typeof(MapEventType),
                    EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF)),
                factory,
                Constant(mapPropertiesToCopy), Constant(arrayPropertiesToCopy));
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new MapEventBeanCopyMethodWithArrayMap(
                mapEventType, eventBeanTypedEventFactory, mapPropertiesToCopy, arrayPropertiesToCopy);
        }
    }
} // end of namespace