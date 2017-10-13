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
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan for lookup using a from-stream event looking up one or more to-streams using a specified lookup plan for each
    /// to-stream.
    /// </summary>
    public class LookupInstructionPlan
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="fromStream">- the stream supplying the lookup event</param>
        /// <param name="fromStreamName">- the stream name supplying the lookup event</param>
        /// <param name="toStreams">- the set of streams to look up in</param>
        /// <param name="lookupPlans">- the plan to use for each stream to look up in</param>
        /// <param name="requiredPerStream">- indicates which of the lookup streams are required to build a result and which are not</param>
        /// <param name="historicalPlans">- plans for use with historical streams</param>
        public LookupInstructionPlan(
            int fromStream,
            string fromStreamName,
            int[] toStreams,
            TableLookupPlan[] lookupPlans,
            HistoricalDataPlanNode[] historicalPlans,
            bool[] requiredPerStream)
        {
            if (toStreams.Length != lookupPlans.Length)
            {
                throw new ArgumentException("Invalid number of lookup plans for each stream");
            }
            if (requiredPerStream.Length < lookupPlans.Length)
            {
                throw new ArgumentException("Invalid required per stream array");
            }
            if ((fromStream < 0) || (fromStream >= requiredPerStream.Length))
            {
                throw new ArgumentException("Invalid from stream");
            }

            FromStream = fromStream;
            FromStreamName = fromStreamName;
            ToStreams = toStreams;
            LookupPlans = lookupPlans;
            HistoricalPlans = historicalPlans;
            RequiredPerStream = requiredPerStream;
        }

        /// <summary>
        /// Constructs the executable from the plan.
        /// </summary>
        /// <param name="statementName">statement name</param>
        /// <param name="statementId">statement id</param>
        /// <param name="annotations">annotations</param>
        /// <param name="indexesPerStream">is the index objects for use in lookups</param>
        /// <param name="streamTypes">is the types of each stream</param>
        /// <param name="streamViews">the viewable representing each stream</param>
        /// <param name="historicalStreamIndexLists">index management for historical streams     @return executable instruction</param>
        /// <param name="viewExternal">virtual data window</param>
        /// <returns>instruction exec</returns>
        public LookupInstructionExec MakeExec(
            string statementName,
            int statementId,
            Attribute[] annotations,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            HistoricalStreamIndexList[] historicalStreamIndexLists,
            VirtualDWView[] viewExternal)
        {
            var strategies = new JoinExecTableLookupStrategy[LookupPlans.Length];
            for (int i = 0; i < LookupPlans.Length; i++)
            {
                if (LookupPlans[i] != null)
                {
                    strategies[i] = LookupPlans[i].MakeStrategy(
                        statementName, statementId, annotations, indexesPerStream, streamTypes, viewExternal);
                }
                else
                {
                    strategies[i] = HistoricalPlans[i].MakeOuterJoinStategy(
                        streamViews, FromStream, historicalStreamIndexLists);
                }
            }
            return new LookupInstructionExec(FromStream, FromStreamName, ToStreams, strategies, RequiredPerStream);
        }

        /// <summary>
        /// Output the planned instruction.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine(
                "LookupInstructionPlan" +
                " fromStream=" + FromStream +
                " fromStreamName=" + FromStreamName +
                " toStreams=" + ToStreams.Render()
                );

            writer.IncrIndent();
            for (int i = 0; i < LookupPlans.Length; i++)
            {
                if (LookupPlans[i] != null)
                {
                    writer.WriteLine("plan " + i + " :" + LookupPlans[i]);
                }
                else
                {
                    writer.WriteLine("plan " + i + " : no lookup plan");
                }
            }
            writer.DecrIndent();
        }

        public void AddIndexes(ISet<TableLookupIndexReqKey> usedIndexes)
        {
            for (int i = 0; i < LookupPlans.Length; i++)
            {
                if (LookupPlans[i] != null)
                {
                    usedIndexes.AddAll(LookupPlans[i].IndexNum);
                }
            }
        }

        public int FromStream { get; private set; }

        public string FromStreamName { get; private set; }

        public int[] ToStreams { get; private set; }

        public TableLookupPlan[] LookupPlans { get; private set; }

        public bool[] RequiredPerStream { get; private set; }

        public HistoricalDataPlanNode[] HistoricalPlans { get; private set; }
    }
} // end of namespace
