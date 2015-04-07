///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Plan for lookup using a from-stream event looking up one or more to-streams using a 
    /// specified lookup plan for each to-stream.
    /// </summary>
    public class LookupInstructionPlan
    {
        private readonly int _fromStream;
        private readonly String _fromStreamName;
        private readonly int[] _toStreams;
        private readonly TableLookupPlan[] _lookupPlans;
        private readonly bool[] _requiredPerStream;
        private readonly HistoricalDataPlanNode[] _historicalPlans;
    
        /// <summary>Ctor. </summary>
        /// <param name="fromStream">the stream supplying the lookup event</param>
        /// <param name="fromStreamName">the stream name supplying the lookup event</param>
        /// <param name="toStreams">the set of streams to look up in</param>
        /// <param name="lookupPlans">the plan to use for each stream to look up in</param>
        /// <param name="requiredPerStream">indicates which of the lookup streams are required to build a result and which are not</param>
        /// <param name="historicalPlans">plans for use with historical streams</param>
        public LookupInstructionPlan(int fromStream, String fromStreamName, int[] toStreams, TableLookupPlan[] lookupPlans, HistoricalDataPlanNode[] historicalPlans, bool[] requiredPerStream)
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
    
            _fromStream = fromStream;
            _fromStreamName = fromStreamName;
            _toStreams = toStreams;
            _lookupPlans = lookupPlans;
            _historicalPlans = historicalPlans;
            _requiredPerStream = requiredPerStream;
        }

        /// <summary>
        /// Constructs the executable from the plan.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="statementId">The statement id.</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="indexesPerStream">is the index objects for use in lookups</param>
        /// <param name="streamTypes">is the types of each stream</param>
        /// <param name="streamViews">the viewable representing each stream</param>
        /// <param name="historicalStreamIndexLists">index management for historical streams</param>
        /// <param name="viewExternal">The view external.</param>
        /// <returns></returns>
        public LookupInstructionExec MakeExec(String statementName, String statementId, Attribute[] annotations, IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream, EventType[] streamTypes, Viewable[] streamViews, HistoricalStreamIndexList[] historicalStreamIndexLists, VirtualDWView[] viewExternal)
        {
            JoinExecTableLookupStrategy[] strategies = new JoinExecTableLookupStrategy[_lookupPlans.Length];
            for (int i = 0; i < _lookupPlans.Length; i++)
            {
                if (_lookupPlans[i] != null)
                {
                    strategies[i] = _lookupPlans[i].MakeStrategy(statementName, statementId, annotations, indexesPerStream, streamTypes, viewExternal);
                }
                else
                {
                    strategies[i] = _historicalPlans[i].MakeOuterJoinStategy(streamViews, _fromStream, historicalStreamIndexLists);
                }
            }
            return new LookupInstructionExec(_fromStream, _fromStreamName, _toStreams, strategies, _requiredPerStream);
        }
    
        /// <summary>Output the planned instruction. </summary>
        /// <param name="writer">to output to</param>
        public void Print(IndentWriter writer)
        {
            writer.WriteLine("LookupInstructionPlan" +
                    " fromStream=" + _fromStream +
                    " fromStreamName=" + _fromStreamName +
                    " toStreams=" + _toStreams.Render()
                    );
    
            writer.IncrIndent();
            for (int i = 0; i < _lookupPlans.Length; i++)
            {
                if (_lookupPlans[i] != null)
                {
                    writer.WriteLine("plan " + i + " :" + _lookupPlans[i]);
                }
                else
                {
                    writer.WriteLine("plan " + i + " : no lookup plan");
                }
            }
            writer.DecrIndent();
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            for (int i = 0; i < _lookupPlans.Length; i++)
            {
                if (_lookupPlans[i] != null)
                {
                    usedIndexes.AddAll(_lookupPlans[i].IndexNum);
                }
            }        
        }

        public int FromStream
        {
            get { return _fromStream; }
        }

        public string FromStreamName
        {
            get { return _fromStreamName; }
        }

        public int[] ToStreams
        {
            get { return _toStreams; }
        }

        public TableLookupPlan[] LookupPlans
        {
            get { return _lookupPlans; }
        }

        public bool[] RequiredPerStream
        {
            get { return _requiredPerStream; }
        }

        public HistoricalDataPlanNode[] HistoricalPlans
        {
            get { return _historicalPlans; }
        }
    }
}
