///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.table.upd
{
    public class TableUpdateStrategyIndexNonUnique : TableUpdateStrategy
    {
        private readonly ISet<string> _affectedIndexNames;
        private readonly EventBeanUpdateHelper _updateHelper;

        public TableUpdateStrategyIndexNonUnique(EventBeanUpdateHelper updateHelper, ISet<string> affectedIndexNames)
        {
            _updateHelper = updateHelper;
            _affectedIndexNames = affectedIndexNames;
        }

        public void UpdateTable(
            ICollection<EventBean> eventsUnsafeIter,
            TableStateInstance instance,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // copy references to array - as it is allowed to pass an index-originating collection
            // and those same indexes are being changed now
            var events = new EventBean[eventsUnsafeIter.Count];
            int count = 0;
            foreach (EventBean @event in eventsUnsafeIter)
            {
                events[count++] = @event;
            }

            // remove from affected indexes
            foreach (string affectedIndexName in _affectedIndexNames)
            {
                EventTable index = instance.GetIndex(affectedIndexName);
                index.Remove(events, instance.AgentInstanceContext);
            }

            // update (no-copy unless original values required)
            foreach (EventBean @event in events)
            {
                eventsPerStream[0] = @event;
                var updatedEvent = (ObjectArrayBackedEventBean) @event;

                // if "initial.property" is part of the assignment expressions, provide initial value event
                if (_updateHelper.IsRequiresStream2InitialValueEvent)
                {
                    var prev = new object[updatedEvent.Properties.Length];
                    Array.Copy(updatedEvent.Properties, 0, prev, 0, prev.Length);
                    eventsPerStream[2] = new ObjectArrayEventBean(prev, updatedEvent.EventType);
                }

                // apply in-place updates
                _updateHelper.UpdateNoCopy(updatedEvent, eventsPerStream, exprEvaluatorContext);
                instance.HandleRowUpdated(updatedEvent);
            }

            // add to affected indexes
            foreach (string affectedIndexName in _affectedIndexNames)
            {
                EventTable index = instance.GetIndex(affectedIndexName);
                index.Add(events, instance.AgentInstanceContext);
            }
        }
    }
}