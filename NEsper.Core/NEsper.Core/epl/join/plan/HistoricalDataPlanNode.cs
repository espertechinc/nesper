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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
	/// <summary>
	/// Query plan for performing a historical data lookup.
	/// <para />Translates into a particular execution for use in regular and outer joins.
	/// </summary>
	public class HistoricalDataPlanNode : QueryPlanNode
	{
	    private readonly int _streamNum;
	    private readonly int _rootStreamNum;
	    private readonly int _lookupStreamNum;
	    private readonly int _numStreams;
	    private readonly ExprNode _outerJoinExprNode;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="streamNum">the historical stream num</param>
	    /// <param name="rootStreamNum">the stream number of the query plan providing incoming events</param>
	    /// <param name="lookupStreamNum">the stream that provides polling/lookup events</param>
	    /// <param name="numStreams">number of streams in join</param>
	    /// <param name="exprNode">outer join expression node or null if none defined</param>
	    public HistoricalDataPlanNode(int streamNum, int rootStreamNum, int lookupStreamNum, int numStreams, ExprNode exprNode)
	    {
	        _streamNum = streamNum;
	        _rootStreamNum = rootStreamNum;
	        _lookupStreamNum = lookupStreamNum;
	        _numStreams = numStreams;
	        _outerJoinExprNode = exprNode;
	    }

	    public override ExecNode MakeExec(string statementName, int statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexLists, VirtualDWView[] viewExternal, ILockable[] tableSecondaryIndexLocks)
	    {
	        var pair = historicalStreamIndexLists[_streamNum].GetStrategy(_lookupStreamNum);
	        var viewable = (HistoricalEventViewable) streamViews[_streamNum];
	        return new HistoricalDataExecNode(viewable, pair.Second, pair.First, _numStreams, _streamNum);
	    }

	    public override void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes) {
	        // none to add
	    }

	    /// <summary>
	    /// Returns the table lookup strategy for use in outer joins.
	    /// </summary>
	    /// <param name="streamViews">all views in join</param>
	    /// <param name="pollingStreamNum">the stream number of the stream looking up into the historical</param>
	    /// <param name="historicalStreamIndexLists">the index management for the historical stream</param>
	    /// <returns>strategy</returns>
	    public HistoricalTableLookupStrategy MakeOuterJoinStategy(Viewable[] streamViews, int pollingStreamNum, HistoricalStreamIndexList[] historicalStreamIndexLists)
	    {
	        var pair = historicalStreamIndexLists[_streamNum].GetStrategy(pollingStreamNum);
	        var viewable = (HistoricalEventViewable) streamViews[_streamNum];
	        return new HistoricalTableLookupStrategy(viewable, pair.Second, pair.First, _numStreams, _streamNum, _rootStreamNum, _outerJoinExprNode == null ? null : _outerJoinExprNode.ExprEvaluator);
	    }

	    public override void Print(IndentWriter writer)
	    {
	        writer.IncrIndent();
	        writer.WriteLine("HistoricalDataPlanNode streamNum=" + _streamNum);
	    }
	}
} // end of namespace
