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
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.outer;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.queryplanouter
{
    /// <summary>
    ///     Plan for lookup using a from-stream event looking up one or more to-streams using a specified lookup plan for each
    ///     to-stream.
    /// </summary>
    public class LookupInstructionPlan
    {
        private readonly HistoricalDataPlanNode[] historicalPlans;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="fromStream">the stream supplying the lookup event</param>
        /// <param name="fromStreamName">the stream name supplying the lookup event</param>
        /// <param name="toStreams">the set of streams to look up in</param>
        /// <param name="lookupPlans">the plan to use for each stream to look up in</param>
        /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
        /// <param name="historicalPlans">plans for use with historical streams</param>
        public LookupInstructionPlan(
            int fromStream,
            string fromStreamName,
            int[] toStreams,
            TableLookupPlan[] lookupPlans,
            HistoricalDataPlanNode[] historicalPlans,
            bool[] requiredPerStream)
        {
            if (toStreams.Length != lookupPlans.Length) {
                throw new ArgumentException("Invalid number of lookup plans for each stream");
            }

            if (requiredPerStream.Length < lookupPlans.Length) {
                throw new ArgumentException("Invalid required per stream array");
            }

            if (fromStream < 0 || fromStream >= requiredPerStream.Length) {
                throw new ArgumentException("Invalid from stream");
            }

            FromStream = fromStream;
            FromStreamName = fromStreamName;
            ToStreams = toStreams;
            LookupPlans = lookupPlans;
            this.historicalPlans = historicalPlans;
            RequiredPerStream = requiredPerStream;
        }

        public int FromStream { get; }

        public string FromStreamName { get; }

        public int[] ToStreams { get; }

        public TableLookupPlan[] LookupPlans { get; }

        public bool[] RequiredPerStream { get; }

        public object[] HistoricalPlans => historicalPlans;

        public LookupInstructionExec MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal)
        {
            var strategies = new JoinExecTableLookupStrategy[LookupPlans.Length];
            for (var i = 0; i < LookupPlans.Length; i++) {
                if (LookupPlans[i] != null) {
                    strategies[i] = LookupPlans[i]
                        .MakeStrategy(agentInstanceContext, indexesPerStream, streamTypes, viewExternal);
                }
                else {
                    strategies[i] = historicalPlans[i].MakeOuterJoinStategy(streamViews);
                }
            }

            return new LookupInstructionExec(FromStream, FromStreamName, ToStreams, strategies, RequiredPerStream);
        }

        /// <summary>
        ///     Output the planned instruction.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionPlan" +
                " fromStream=" +
                FromStream +
                " fromStreamName=" +
                FromStreamName +
                " toStreams=" +
                ToStreams.RenderAny()
            );

            writer.IncrIndent();
            for (var i = 0; i < LookupPlans.Length; i++) {
                if (LookupPlans[i] != null) {
                    writer.WriteLine("plan " + i + " :" + LookupPlans[i]);
                }
                else {
                    writer.WriteLine("plan " + i + " : no lookup plan");
                }
            }

            writer.DecrIndent();
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            for (var i = 0; i < LookupPlans.Length; i++) {
                if (LookupPlans[i] != null) {
                    usedIndexes.AddAll(Arrays.AsList(LookupPlans[i].IndexNum));
                }
            }
        }
    }
} // end of namespace