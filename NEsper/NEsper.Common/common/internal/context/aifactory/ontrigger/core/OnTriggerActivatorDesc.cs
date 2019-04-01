///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
	public class OnTriggerActivatorDesc {
	    private readonly ViewableActivatorForge activator;
	    private readonly string triggerEventTypeName;
	    private readonly EventType activatorResultEventType;

	    public OnTriggerActivatorDesc(ViewableActivatorForge activator, string triggerEventTypeName, EventType activatorResultEventType) {
	        this.activator = activator;
	        this.triggerEventTypeName = triggerEventTypeName;
	        this.activatorResultEventType = activatorResultEventType;
	    }

	    public ViewableActivatorForge Activator {
	        get => activator;
	    }

	    public string TriggerEventTypeName {
	        get => triggerEventTypeName;
	    }

	    public EventType ActivatorResultEventType {
	        get => activatorResultEventType;
	    }
	}
} // end of namespace