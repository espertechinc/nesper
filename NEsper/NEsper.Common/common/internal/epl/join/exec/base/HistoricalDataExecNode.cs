///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    ///     Execution node for executing a join or outer join against a historical data source,
    ///     using an lookup strategy for looking up into cached indexes, and an indexing strategy for indexing poll results
    ///     for future lookups.
    /// </summary>
    public class HistoricalDataExecNode : ExecNode
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly HistoricalEventViewable historicalEventViewable;
        private readonly int historicalStreamNumber;
        private readonly PollResultIndexingStrategy indexingStrategy;
        private readonly HistoricalIndexLookupStrategy indexLookupStrategy;

        private readonly EventBean[][] lookupRows1Event;
        private readonly int numStreams;

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
            this.historicalEventViewable = historicalEventViewable;
            this.indexingStrategy = indexingStrategy;
            this.indexLookupStrategy = indexLookupStrategy;
            this.numStreams = numStreams;
            this.historicalStreamNumber = historicalStreamNumber;

            lookupRows1Event = new EventBean[1][];
            lookupRows1Event[0] = new EventBean[numStreams];
        }

        public override void Process(
            EventBean lookupEvent,
            EventBean[] prefillPath,
            ICollection<EventBean[]> result,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            lookupRows1Event[0] = prefillPath;
            var indexPerLookupRow = historicalEventViewable.Poll(
                lookupRows1Event,
                indexingStrategy,
                exprEvaluatorContext);

            foreach (var index in indexPerLookupRow) {
                // Using the index, determine a subset of the whole indexed table to process, unless
                // the strategy is a full table scan
                var subsetIter = indexLookupStrategy.Lookup(lookupEvent, index, exprEvaluatorContext);

                if (subsetIter != null) {
                    // Add each row to the join result or, for outer joins, run through the outer join filter
                    while (subsetIter.MoveNext()) {
                        var resultRow = new EventBean[numStreams];
                        Array.Copy(prefillPath, 0, resultRow, 0, numStreams);
                        resultRow[historicalStreamNumber] = subsetIter.Current;
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