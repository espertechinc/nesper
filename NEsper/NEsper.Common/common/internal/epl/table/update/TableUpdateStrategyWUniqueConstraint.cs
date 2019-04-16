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
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.updatehelper;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.update
{
    public class TableUpdateStrategyWUniqueConstraint : TableUpdateStrategy
    {
        private readonly EventBeanUpdateHelperNoCopy updateHelper;
        private readonly ISet<string> affectedIndexNames;

        public TableUpdateStrategyWUniqueConstraint(
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
            EventBean[] events = new EventBean[eventsUnsafeIter.Count];
            int count = 0;
            foreach (EventBean @event in eventsUnsafeIter) {
                events[count++] = @event;
            }

            // remove from affected indexes
            foreach (string affectedIndexName in affectedIndexNames) {
                EventTable index = instance.GetIndex(affectedIndexName);
                index.Remove(events, exprEvaluatorContext);
            }

            // copy event data, since we are updating unique keys and must guarantee rollback (no half update)
            object[][] previousData = new object[events.Length][];

            // copy and then update
            for (int i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];

                // copy non-aggregated value references
                ObjectArrayBackedEventBean updatedEvent = (ObjectArrayBackedEventBean) events[i];
                object[] prev = new object[updatedEvent.Properties.Length];
                Array.Copy(updatedEvent.Properties, 0, prev, 0, prev.Length);
                previousData[i] = prev;

                // if "initial.property" is part of the assignment expressions, provide initial value event
                if (updateHelper.IsRequiresStream2InitialValueEvent) {
                    eventsPerStream[2] = new ObjectArrayEventBean(prev, updatedEvent.EventType);
                }

                // apply in-place updates
                instance.HandleRowUpdateKeyBeforeUpdate(updatedEvent);
                updateHelper.UpdateNoCopy(updatedEvent, eventsPerStream, exprEvaluatorContext);
                instance.HandleRowUpdateKeyAfterUpdate(updatedEvent);
            }

            // add to affected indexes
            try {
                foreach (string affectedIndexName in affectedIndexNames) {
                    EventTable index = instance.GetIndex(affectedIndexName);
                    index.Add(events, exprEvaluatorContext);
                }
            }
            catch (EPException ex) {
                // rollback
                // remove updated events
                foreach (string affectedIndexName in affectedIndexNames) {
                    EventTable index = instance.GetIndex(affectedIndexName);
                    index.Remove(events, exprEvaluatorContext);
                }

                // rollback change to events
                for (int i = 0; i < events.Length; i++) {
                    ObjectArrayBackedEventBean oa = (ObjectArrayBackedEventBean) events[i];
                    oa.PropertyValues = previousData[i];
                }

                // add old events
                foreach (string affectedIndexName in affectedIndexNames) {
                    EventTable index = instance.GetIndex(affectedIndexName);
                    index.Add(events, exprEvaluatorContext);
                }

                throw;
            }
        }
    }
} // end of namespace