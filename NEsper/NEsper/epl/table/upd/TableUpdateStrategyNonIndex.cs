///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.upd
{
    public class TableUpdateStrategyNonIndex : TableUpdateStrategy
    {
        private readonly EventBeanUpdateHelper _updateHelper;
    
        public TableUpdateStrategyNonIndex(EventBeanUpdateHelper updateHelper)
        {
            _updateHelper = updateHelper;
        }
    
        public void UpdateTable(ICollection<EventBean> eventsUnsafeIter, TableStateInstance instance, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            // update (no-copy unless original values required)
            foreach (var @event in eventsUnsafeIter) {
                eventsPerStream[0] = @event;
                var updatedEvent = (ObjectArrayBackedEventBean) @event;
    
                // if "initial.property" is part of the assignment expressions, provide initial value event
                if (_updateHelper.IsRequiresStream2InitialValueEvent) {
                    var prev = new object[updatedEvent.Properties.Length];
                    Array.Copy(updatedEvent.Properties, 0, prev, 0, prev.Length);
                    eventsPerStream[2] = new ObjectArrayEventBean(prev, updatedEvent.EventType);
                }
    
                // apply in-place updates
                _updateHelper.UpdateNoCopy(updatedEvent, eventsPerStream, exprEvaluatorContext);
                instance.HandleRowUpdated(updatedEvent);
            }
        }
    }
}
