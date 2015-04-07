///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.mgmt
{
    public class TableStateInstanceUngrouped
        : TableStateInstance
        , IEnumerable<EventBean>
    {
        private readonly Atomic<ObjectArrayBackedEventBean> _eventReference;

        public TableStateInstanceUngrouped(TableMetadata tableMetadata, AgentInstanceContext agentInstanceContext)
            : base(tableMetadata, agentInstanceContext)
        {
            _eventReference = new Atomic<ObjectArrayBackedEventBean>(null);
        }

        public override IEnumerable<EventBean> IterableTableScan
        {
            get
            {
                EventBean value;
                if (_eventReference != null && ((value = _eventReference.Get()) != null))
                    yield return value;
            }
        }

        public override void AddEvent(EventBean theEvent)
        {
            if (_eventReference.Get() != null) {
                throw new EPException("Unique index violation, table '" + TableMetadata.TableName + "' " +
                        "is a declared to hold a single un-keyed row");
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableAddEvent(theEvent); }
            _eventReference.Set((ObjectArrayBackedEventBean) theEvent);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableAddEvent(); }
        }
    
        public override void DeleteEvent(EventBean matchingEvent)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QTableDeleteEvent(matchingEvent); }
            _eventReference.Set(null);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ATableDeleteEvent(); }
        }

        public Atomic<ObjectArrayBackedEventBean> EventReference
        {
            get { return _eventReference; }
        }

        public override void AddExplicitIndex(CreateIndexDesc spec)
        {
            throw new ExprValidationException("Tables without primary key column(s) do not allow creating an index");
        }
    
        public override EventTable GetIndex(string indexName)
        {
            if (indexName.Equals(TableMetadata.TableName)) {
                var org = new EventTableOrganization(TableMetadata.TableName,
                        true, false, 0, new string[0], EventTableOrganization.EventTableOrganizationType.UNORGANIZED);
                return new SingleReferenceEventTable(org, _eventReference);
            }
            throw new IllegalStateException("Invalid index requested '" + indexName + "'");
        }

        public override string[] SecondaryIndexes
        {
            get { return new string[0]; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var theEvent = _eventReference.Get();
            if (theEvent != null)
                yield return theEvent;
        }
    
        public override void ClearEvents()
        {
            _eventReference.Set(null);
        }

        public override ICollection<EventBean> EventCollection
        {
            get
            {
                var theEvent = _eventReference.Get();
                if (theEvent == null)
                {
                    return Collections.GetEmptyList<EventBean>();
                }
                return Collections.SingletonList<EventBean>(theEvent);
            }
        }

        public override int RowCount
        {
            get { return _eventReference.Get() == null ? 0 : 1; }
        }

        public override ObjectArrayBackedEventBean GetCreateRowIntoTable(object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = _eventReference.Get();
            if (bean != null) {
                return bean;
            }
            
            var row = TableMetadata.RowFactory.MakeOA(exprEvaluatorContext.AgentInstanceId, groupByKey, null);
            AddEvent(row);
            return row;
        }
    }
}
