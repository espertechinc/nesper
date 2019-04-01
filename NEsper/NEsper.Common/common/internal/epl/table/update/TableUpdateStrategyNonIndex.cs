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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.update
{
	public class TableUpdateStrategyNonIndex : TableUpdateStrategy {

	    private readonly EventBeanUpdateHelperNoCopy updateHelper;

	    public TableUpdateStrategyNonIndex(EventBeanUpdateHelperNoCopy updateHelper) {
	        this.updateHelper = updateHelper;
	    }

	    public void UpdateTable(ICollection<EventBean> eventsUnsafeIter, TableInstance instance, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext) {
	        // update (no-copy unless original values required)
	        foreach (EventBean @event in eventsUnsafeIter) {
	            eventsPerStream[0] = @event;
	            ObjectArrayBackedEventBean updatedEvent = (ObjectArrayBackedEventBean) @event;

	            // if "initial.property" is part of the assignment expressions, provide initial value event
	            if (updateHelper.IsRequiresStream2InitialValueEvent) {
	                object[] prev = new object[updatedEvent.Properties.Length];
	                Array.Copy(updatedEvent.Properties, 0, prev, 0, prev.Length);
	                eventsPerStream[2] = new ObjectArrayEventBean(prev, updatedEvent.EventType);
	            }

	            // apply in-place updates
	            updateHelper.UpdateNoCopy(updatedEvent, eventsPerStream, exprEvaluatorContext);
	            instance.HandleRowUpdated(updatedEvent);
	        }
	    }
	}
} // end of namespace