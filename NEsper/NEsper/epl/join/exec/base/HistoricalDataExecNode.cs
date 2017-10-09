///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    ///     Execution node for executing a join or outer join against a historical data source,
    ///     using an lookup strategy for looking up into cached indexes, and an indexing strategy for indexing poll results
    ///     for future lookups.
    /// </summary>
    public class HistoricalDataExecNode : ExecNode
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly HistoricalEventViewable _historicalEventViewable;
        private readonly int _historicalStreamNumber;
        private readonly HistoricalIndexLookupStrategy _indexLookupStrategy;
        private readonly PollResultIndexingStrategy _indexingStrategy;
        private readonly EventBean[][] _lookupRows1Event;
        private readonly int _numStreams;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="historicalEventViewable">the view of the historical</param>
        /// <param name="indexingStrategy">the strategy to index poll result for future use</param>
        /// <param name="indexLookupStrategy">the strategy to use past indexed results</param>
        /// <param name="numStreams">the number of streams in the join</param>
        /// <param name="historicalStreamNumber">the stream number of the historical</param>
        public HistoricalDataExecNode(
            HistoricalEventViewable historicalEventViewable,
            PollResultIndexingStrategy indexingStrategy,
            HistoricalIndexLookupStrategy indexLookupStrategy,
            int numStreams,
            int historicalStreamNumber)
        {
            _historicalEventViewable = historicalEventViewable;
            _indexingStrategy = indexingStrategy;
            _indexLookupStrategy = indexLookupStrategy;
            _numStreams = numStreams;
            _historicalStreamNumber = historicalStreamNumber;

            _lookupRows1Event = new EventBean[1][];
            _lookupRows1Event[0] = new EventBean[numStreams];
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _lookupRows1Event[0] = prefillPath;
            EventTable[][] indexPerLookupRow = _historicalEventViewable.Poll(
                _lookupRows1Event, _indexingStrategy, exprEvaluatorContext);

            for(int ii = 0; ii < indexPerLookupRow.Length; ii++)
            {
                var index = indexPerLookupRow[ii];

                // Using the index, determine a subset of the whole indexed table to process, unless
                // the strategy is a full table scan
                IEnumerator<EventBean> subsetIter = _indexLookupStrategy.Lookup(
                    lookupEvent, index, exprEvaluatorContext);

                if (subsetIter != null)
                {
                    // Add each row to the join result or, for outer joins, run through the outer join filter
                    for (; subsetIter.MoveNext();)
                    {
                        var resultRow = new EventBean[_numStreams];
                        Array.Copy(prefillPath, 0, resultRow, 0, _numStreams);
                        resultRow[_historicalStreamNumber] = subsetIter.Current;
                        result.Add(resultRow);
                    }
                }
            }
        }

        public override void Print(IndentWriter writer)
        {
            writer.WriteLine("HistoricalDataExecNode");
        }
    }
} // end of namespace