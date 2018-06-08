///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.table.mgmt
{
	public class TableStateInstanceUngroupedImpl 
        : TableStateInstance 
        , TableStateInstanceUngrouped
        , IEnumerable<EventBean>
    {
        private readonly Atomic<ObjectArrayBackedEventBean> _eventReference;

	    public TableStateInstanceUngroupedImpl(
	        TableMetadata tableMetadata, 
	        AgentInstanceContext agentInstanceContext,
	        IReaderWriterLockManager rwLockManager)
            : base(tableMetadata, agentInstanceContext, rwLockManager)
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

	    public ObjectArrayBackedEventBean EventUngrouped
	    {
	        get { return _eventReference.Get(); }
	    }

        public override void AddExplicitIndex(string explicitIndexName, QueryPlanIndexItem explicitIndexDesc, bool isRecoveringResilient, bool allowIndexExists)
        {
	        throw new ExprValidationException("Tables without primary key column(s) do not allow creating an index");
	    }

	    public override EventTable GetIndex(string indexName)
	    {
	        var tableMetadata = TableMetadata;
	        if (indexName == tableMetadata.TableName)
            {
	            var org = new EventTableOrganization(tableMetadata.TableName, true, false, 0, new string[0], EventTableOrganizationType.UNORGANIZED);
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
	        yield return _eventReference.Get();
	    }

	    public override void ClearInstance()
        {
	        ClearEvents();
	    }

	    public override void DestroyInstance()
        {
	        ClearEvents();
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
	        var row = TableMetadata.RowFactory.MakeOA(exprEvaluatorContext.AgentInstanceId, groupByKey, null, AggregationServicePassThru);
	        AddEvent(row);
	        return row;
	    }

	    public override AggregationServicePassThru AggregationServicePassThru
	    {
	        get { return null; }
	    }

	    public void ClearEvents()
        {
	        _eventReference.Set(null);
	    }
	}
} // end of namespace
