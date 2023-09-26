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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.table.core
{
    public class TableInstanceUngroupedImpl : TableInstanceUngroupedBase
    {
        private readonly Atomic<ObjectArrayBackedEventBean> eventReference;

        public TableInstanceUngroupedImpl(
            Table table,
            AgentInstanceContext agentInstanceContext)
            : base(table, agentInstanceContext)
        {
            eventReference = new Atomic<ObjectArrayBackedEventBean>(null);
        }

        public override ICollection<EventBean> EventCollection {
            get {
                EventBean @event = eventReference.Get();
                if (@event == null) {
                    return Collections.GetEmptyList<EventBean>();
                }

                return Collections.SingletonList(@event);
            }
        }

        public override ObjectArrayBackedEventBean EventUngrouped => eventReference.Get();

        public override IEnumerable<EventBean> IterableTableScan {
            get {
                var currentValue = eventReference.Get();
                if (currentValue != null) {
                    yield return currentValue;
                }
            }
        }

        public override void AddEvent(EventBean @event)
        {
            if (@event.EventType != table.MetaData.InternalEventType) {
                throw new IllegalStateException("Unexpected event type for add: " + @event.EventType.Name);
            }

            if (eventReference.Get() != null) {
                throw new EPException(
                    "Unique index violation, table '" +
                    table.MetaData.TableName +
                    "' " +
                    "is a declared to hold a single un-keyed row");
            }

            agentInstanceContext.InstrumentationProvider.QTableAddEvent(@event);
            eventReference.Set((ObjectArrayBackedEventBean)@event);
            agentInstanceContext.InstrumentationProvider.ATableAddEvent();
        }

        public override ObjectArrayBackedEventBean GetCreateRowIntoTable(ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = eventReference.Get();
            if (bean != null) {
                return bean;
            }

            return CreateRowIntoTable();
        }

        public override void ClearInstance()
        {
            ClearEvents();
        }

        public override void Destroy()
        {
            ClearEvents();
        }

        private void ClearEvents()
        {
            eventReference.Set(null);
        }

        public override void DeleteEvent(EventBean matchingEvent)
        {
            agentInstanceContext.InstrumentationProvider.QTableDeleteEvent(matchingEvent);
            eventReference.Set(null);
            agentInstanceContext.InstrumentationProvider.ATableDeleteEvent();
        }

        public override EventTable GetIndex(
            string indexName,
            string indexModuleName)
        {
            if (indexName.Equals(table.Name)) {
                var org = new EventTableOrganization(
                    table.Name,
                    true,
                    false,
                    0,
                    Array.Empty<string>(),
                    EventTableOrganizationType.UNORGANIZED);
                return new SingleReferenceEventTable(org, eventReference);
            }

            throw new IllegalStateException("Invalid index requested '" + indexName + "'");
        }

        public override void HandleRowUpdated(ObjectArrayBackedEventBean updatedEvent)
        {
            // no action
            if (agentInstanceContext.InstrumentationProvider.Activated()) {
                agentInstanceContext.InstrumentationProvider.QTableUpdatedEvent(updatedEvent);
                agentInstanceContext.InstrumentationProvider.ATableUpdatedEvent();
            }
        }

        public override void HandleRowUpdateKeyBeforeUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
        }

        public override void HandleRowUpdateKeyAfterUpdate(ObjectArrayBackedEventBean updatedEvent)
        {
        }
    }
} // end of namespace