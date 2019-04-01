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
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.outer;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
	/// <summary>
	/// Plan for lookup using a from-stream event looking up one or more to-streams using a specified lookup plan for each
	/// to-stream.
	/// </summary>
	public class LookupInstructionPlan {
	    private readonly int fromStream;
	    private readonly string fromStreamName;
	    private readonly int[] toStreams;
	    private readonly TableLookupPlan[] lookupPlans;
	    private readonly bool[] requiredPerStream;
	    private readonly HistoricalDataPlanNode[] historicalPlans;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="fromStream">the stream supplying the lookup event</param>
	    /// <param name="fromStreamName">the stream name supplying the lookup event</param>
	    /// <param name="toStreams">the set of streams to look up in</param>
	    /// <param name="lookupPlans">the plan to use for each stream to look up in</param>
	    /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
	    /// <param name="historicalPlans">plans for use with historical streams</param>
	    public LookupInstructionPlan(int fromStream, string fromStreamName, int[] toStreams, TableLookupPlan[] lookupPlans, HistoricalDataPlanNode[] historicalPlans, bool[] requiredPerStream) {
	        if (toStreams.Length != lookupPlans.Length) {
	            throw new ArgumentException("Invalid number of lookup plans for each stream");
	        }
	        if (requiredPerStream.Length < lookupPlans.Length) {
	            throw new ArgumentException("Invalid required per stream array");
	        }
	        if ((fromStream < 0) || (fromStream >= requiredPerStream.Length)) {
	            throw new ArgumentException("Invalid from stream");
	        }

	        this.fromStream = fromStream;
	        this.fromStreamName = fromStreamName;
	        this.toStreams = toStreams;
	        this.lookupPlans = lookupPlans;
	        this.historicalPlans = historicalPlans;
	        this.requiredPerStream = requiredPerStream;
	    }

	    public LookupInstructionExec MakeExec(AgentInstanceContext agentInstanceContext, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, VirtualDWView[] viewExternal) {
	        JoinExecTableLookupStrategy[] strategies = new JoinExecTableLookupStrategy[lookupPlans.Length];
	        for (int i = 0; i < lookupPlans.Length; i++) {
	            if (lookupPlans[i] != null) {
	                strategies[i] = lookupPlans[i].MakeStrategy(agentInstanceContext, indexesPerStream, streamTypes, viewExternal);
	            } else {
	                strategies[i] = historicalPlans[i].MakeOuterJoinStategy(streamViews);
	            }
	        }
	        return new LookupInstructionExec(fromStream, fromStreamName, toStreams, strategies, requiredPerStream);
	    }

	    /// <summary>
	    /// Output the planned instruction.
	    /// </summary>
	    /// <param name="writer">to output to</param>
	    public void Print(IndentWriter writer) {
	        writer.WriteLine("LookupInstructionPlan" +
	                " fromStream=" + fromStream +
	                " fromStreamName=" + fromStreamName +
	                " toStreams=" + CompatExtensions.RenderAny(toStreams)
	        );

	        writer.IncrIndent();
	        for (int i = 0; i < lookupPlans.Length; i++) {
	            if (lookupPlans[i] != null) {
	                writer.WriteLine("plan " + i + " :" + lookupPlans[i].ToString());
	            } else {
	                writer.WriteLine("plan " + i + " : no lookup plan");
	            }
	        }
	        writer.DecrIndent();
	    }

	    public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes) {
	        for (int i = 0; i < lookupPlans.Length; i++) {
	            if (lookupPlans[i] != null) {
	                usedIndexes.AddAll(Arrays.AsList(lookupPlans[i].IndexNum));
	            }
	        }
	    }

	    public int FromStream
	    {
	        get => fromStream;
	    }

	    public string FromStreamName
	    {
	        get => fromStreamName;
	    }

	    public int[] ToStreams
	    {
	        get => toStreams;
	    }

	    public TableLookupPlan[] LookupPlans
	    {
	        get => lookupPlans;
	    }

	    public bool[] RequiredPerStream
	    {
	        get => requiredPerStream;
	    }

	    public object[] HistoricalPlans
	    {
	        get => historicalPlans;
	    }
	}
} // end of namespace