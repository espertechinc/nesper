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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.support.epl
{
	public class SupportQueryPlanNode : QueryPlanNode
	{
	    private readonly string _id;

	    public SupportQueryPlanNode(string id)
	    {
	        _id = id;
	    }

	    public override ExecNode MakeExec(string statementName, int statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexLists, VirtualDWView[] viewExternal, ILockable[] tableSecondaryIndexLocks)
	    {
	        return new SupportQueryExecNode(_id);
	    }

	    public override void Print(IndentWriter writer)
	    {
	        writer.WriteLine(GetType().FullName);
	    }

	    public override void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes)
        {
	    }
	}
} // end of namespace
