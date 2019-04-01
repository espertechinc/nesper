///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
	/// <summary>
	/// Copy method for Map-underlying events.
	/// </summary>
	public class MapEventBeanCopyMethodForge : EventBeanCopyMethodForge {
	    private readonly MapEventType mapEventType;
	    private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="mapEventType">map event type</param>
	    /// <param name="eventBeanTypedEventFactory">for copying events</param>
	    public MapEventBeanCopyMethodForge(MapEventType mapEventType, EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        this.mapEventType = mapEventType;
	        this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	    }

	    public EventBean Copy(EventBean theEvent) {
	        MappedEventBean mapped = (MappedEventBean) theEvent;
	        IDictionary<string, object> props = mapped.Properties;
	        return eventBeanTypedEventFactory.AdapterForTypedMap(new Dictionary<string, object>(props), mapEventType);
	    }

	    public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope) {
	        CodegenExpressionField factory = classScope.AddOrGetFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
	        return NewInstance(typeof(MapEventBeanCopyMethod),
	                Cast(typeof(MapEventType), EventTypeUtility.ResolveTypeCodegen(mapEventType, EPStatementInitServicesConstants.REF)),
	                factory);
	    }

	    public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        return new MapEventBeanCopyMethod(mapEventType, eventBeanTypedEventFactory);
	    }
	}
} // end of namespace