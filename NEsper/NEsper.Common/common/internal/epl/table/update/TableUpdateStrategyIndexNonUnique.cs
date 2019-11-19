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

namespace com.espertech.esper.common.@internal.epl.table.update
{
    public class TableUpdateStrategyIndexNonUnique : TableUpdateStrategy
    {
        private readonly ISet<string> affectedIndexNames;

        private readonly EventBeanUpdateHelperNoCopy updateHelper;

        public TableUpdateStrategyIndexNonUnique(
            EventBeanUpdateHelperNoCopy updateHelper,
            ISet<string> affectedIndexNames)
        {
            this.updateHelper = updateHelper;
            this.affectedIndexNames = affectedIndexNames;
        }

        public void UpdateTable(
            ICollection<EventBean> eventsUnsafeIter,
            TableInstance instance,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // copy references to array - as it is allowed to pass an index-originating collection
            // and those same indexes are being changed now
            var events = new EventBean[eventsUnsafeIter.Count];
            var count = 0;
            foreach (var @event in eventsUnsafeIter) {
                events[count++] = @event;
            }

            // remove from affected indexes
            foreach (var affectedIndexName in affectedIndexNames) {
                var index = instance.GetIndex(affectedIndexName);
                index.Remove(events, instance.AgentInstanceContext);
            }

            // update (no-copy unless original values required)
            foreach (var @event in events) {
                eventsPerStream[0] = @event;
                var updatedEvent = (ObjectArrayBackedEventBean) @event;

                // if "initial.property" is part of the assignment expressions, provide initial value event
                if (updateHelper.IsRequiresStream2InitialValueEvent) {
                    var prev = new object[updatedEvent.Properties.Length];
                    Array.Copy(updatedEvent.Properties, 0, prev, 0, prev.Length);
                    eventsPerStream[2] = new ObjectArrayEventBean(prev, updatedEvent.EventType);
                }

                // apply in-place updates
                updateHelper.UpdateNoCopy(updatedEvent, eventsPerStream, exprEvaluatorContext);
                instance.HandleRowUpdated(updatedEvent);
            }

            // add to affected indexes
            foreach (var affectedIndexName in affectedIndexNames) {
                var index = instance.GetIndex(affectedIndexName);
                index.Add(events, instance.AgentInstanceContext);
            }
        }
    }
} // end of namespace