///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
	/// <summary>
	/// Plan for lookup using a from-stream event looking up one or more to-streams using a specified lookup plan for each
	/// to-stream.
	/// </summary>
	public class LookupInstructionPlanForge : CodegenMakeable<SAIFFInitializeSymbol> {
	    private readonly int fromStream;
	    private readonly string fromStreamName;
	    private readonly int[] toStreams;
	    private readonly TableLookupPlanForge[] lookupPlans;
	    private readonly bool[] requiredPerStream;
	    private readonly HistoricalDataPlanNodeForge[] historicalPlans;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="fromStream">the stream supplying the lookup event</param>
	    /// <param name="fromStreamName">the stream name supplying the lookup event</param>
	    /// <param name="toStreams">the set of streams to look up in</param>
	    /// <param name="lookupPlans">the plan to use for each stream to look up in</param>
	    /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
	    /// <param name="historicalPlans">plans for use with historical streams</param>
	    public LookupInstructionPlanForge(int fromStream, string fromStreamName, int[] toStreams, TableLookupPlanForge[] lookupPlans, HistoricalDataPlanNodeForge[] historicalPlans, bool[] requiredPerStream) {
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

	    public CodegenExpression Make(CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope) {
	        return NewInstance(typeof(LookupInstructionPlan),
	                Constant(fromStream),
	                Constant(fromStreamName),
	                Constant(toStreams),
	                CodegenMakeableUtil.MakeArray("lookupPlans", typeof(TableLookupPlan), lookupPlans, this.GetType(), parent, symbols, classScope),
	                CodegenMakeableUtil.MakeArray("historicalPlans", typeof(HistoricalDataPlanNode), historicalPlans, this.GetType(), parent, symbols, classScope),
	                Constant(requiredPerStream));
	    }

	    public int FromStream {
	        get => fromStream;
	    }

	    public string FromStreamName {
	        get => fromStreamName;
	    }

	    public int[] GetToStreams() {
	        return toStreams;
	    }

	    public TableLookupPlanForge[] GetLookupPlans() {
	        return lookupPlans;
	    }

	    public bool[] GetRequiredPerStream() {
	        return requiredPerStream;
	    }

	    public HistoricalDataPlanNodeForge[] GetHistoricalPlans() {
	        return historicalPlans;
	    }
	}
} // end of namespace