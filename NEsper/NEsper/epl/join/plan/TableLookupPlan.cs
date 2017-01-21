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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.join.plan
{
	/// <summary>
	/// Abstract specification on how to perform a table lookup.
	/// </summary>
	public abstract class TableLookupPlan
	{
	    public abstract JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes);
	    public abstract TableLookupKeyDesc KeyDescriptor { get; }

	    /// <summary>
	    /// Instantiates the lookup plan into a execution strategy for the lookup.
	    /// </summary>
	    /// <param name="statementName">Name of the statement.</param>
	    /// <param name="statementId">The statement identifier.</param>
	    /// <param name="accessedByStmtAnnotations">The accessed by statement annotations.</param>
	    /// <param name="indexesPerStream">tables for each stream</param>
	    /// <param name="eventTypes">types of events in stream</param>
	    /// <param name="viewExternals">The view externals.</param>
	    /// <returns>
	    /// lookup strategy instance
	    /// </returns>
	    public JoinExecTableLookupStrategy MakeStrategy(string statementName, int statementId, Attribute[] accessedByStmtAnnotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] eventTypes, VirtualDWView[] viewExternals)
        {
	        var eventTables = new EventTable[IndexNum.Length];
	        for (var i = 0; i < IndexNum.Length; i++) {
	            eventTables[i] = indexesPerStream[IndexedStream].Get(IndexNum[i]);
	        }
	        if (viewExternals[IndexedStream] != null) {
	            return viewExternals[IndexedStream].GetJoinLookupStrategy(statementName, statementId, accessedByStmtAnnotations, eventTables, KeyDescriptor, LookupStream);
	        }
	        return MakeStrategyInternal(eventTables, eventTypes);
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="lookupStream">stream number of stream that supplies event to be used to look up</param>
	    /// <param name="indexedStream">stream number of stream that is being access via index/table</param>
	    /// <param name="indexNum">index to use for lookup</param>
	    protected TableLookupPlan(int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexNum)
	    {
	        LookupStream = lookupStream;
	        IndexedStream = indexedStream;
	        IndexNum = indexNum;
	    }

	    /// <summary>
	    /// Returns the lookup stream.
	    /// </summary>
	    /// <value>lookup stream</value>
	    public int LookupStream { get; private set; }

	    /// <summary>
	    /// Returns indexed stream.
	    /// </summary>
	    /// <value>indexed stream</value>
	    public int IndexedStream { get; private set; }

	    /// <summary>
	    /// Returns index number to use for looking up in.
	    /// </summary>
	    /// <value>index number</value>
	    public TableLookupIndexReqKey[] IndexNum { get; private set; }

	    public override string ToString()
	    {
	        return "lookupStream=" + LookupStream +
	               " indexedStream=" + IndexedStream +
	               " indexNum=" + IndexNum.Render();
	    }
	}
} // end of namespace
