///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.strategy;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    /// Query strategy for use with <see cref="HistoricalEventViewable" /> to perform lookup for a given stream using the poll method on a viewable.
    /// </summary>
    public class HistoricalDataQueryStrategy : QueryStrategy
    {
        private readonly HistoricalEventViewable _historicalEventViewable;
        private readonly int _historicalStreamNumber;
        private readonly HistoricalIndexLookupStrategy _indexLookupStrategy;
        private readonly bool _isOuterJoin;
        private readonly EventBean[][] _lookupRows1Event;
        private readonly int _myStreamNumber;
        private readonly ExprEvaluator _outerJoinCompareNode;
        private readonly PollResultIndexingStrategy _pollResultIndexingStrategy;

        /// <summary>Ctor. </summary>
        /// <param name="myStreamNumber">is the strategy's stream number</param>
        /// <param name="historicalStreamNumber">is the stream number of the view to be polled</param>
        /// <param name="historicalEventViewable">is the view to be polled from</param>
        /// <param name="isOuterJoin">is this is an outer join</param>
        /// <param name="outerJoinCompareNode">is the node to perform the on-comparison for outer joins</param>
        /// <param name="indexLookupStrategy">the strategy to use for limiting the cache result setto only those rows that match filter criteria </param>
        /// <param name="pollResultIndexingStrategy">the strategy for indexing poll-results such that astrategy can use the index instead of a full table scan to resolve rows </param>
        public HistoricalDataQueryStrategy(
            int myStreamNumber,
            int historicalStreamNumber,
            HistoricalEventViewable historicalEventViewable,
            bool isOuterJoin,
            ExprEvaluator outerJoinCompareNode,
            HistoricalIndexLookupStrategy indexLookupStrategy,
            PollResultIndexingStrategy pollResultIndexingStrategy)
        {
            _myStreamNumber = myStreamNumber;
            _historicalStreamNumber = historicalStreamNumber;
            _historicalEventViewable = historicalEventViewable;
            _isOuterJoin = isOuterJoin;
            _outerJoinCompareNode = outerJoinCompareNode;

            _lookupRows1Event = new EventBean[1][];
            _lookupRows1Event[0] = new EventBean[2];

            _indexLookupStrategy = indexLookupStrategy;
            _pollResultIndexingStrategy = pollResultIndexingStrategy;
        }

        #region QueryStrategy Members

        public void Lookup(
            EventBean[] lookupEvents,
            ICollection<MultiKeyArrayOfKeys<EventBean>> joinSet,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean[][] lookupRows;

            // If looking up a single event, reuse the buffered array
            if (lookupEvents.Length == 1) {
                lookupRows = _lookupRows1Event;
                lookupRows[0][_myStreamNumber] = lookupEvents[0];
            }
            else {
                // Prepare rows with each row Count events where Count is the number of streams
                lookupRows = new EventBean[lookupEvents.Length][];
                for (int i = 0; i < lookupEvents.Length; i++) {
                    lookupRows[i] = new EventBean[2];
                    lookupRows[i][_myStreamNumber] = lookupEvents[i];
                }
            }

            EventTable[][] indexPerLookupRow = _historicalEventViewable.Poll(
                lookupRows,
                _pollResultIndexingStrategy,
                exprEvaluatorContext);

            int count = 0;
            foreach (EventTable[] index in indexPerLookupRow) {
                // Using the index, determine a subset of the whole indexed table to process, unless
                // the strategy is a full table scan
                IEnumerator<EventBean> subsetIter =
                    _indexLookupStrategy.Lookup(lookupEvents[count], index, exprEvaluatorContext);

                // Ensure that the subset enumerator is advanced; assuming that there
                // was an iterator at all.
                bool subsetIterAdvanced =
                    (subsetIter != null) &&
                    (subsetIter.MoveNext());

                // In an outer join
                if (_isOuterJoin && !subsetIterAdvanced) {
                    var resultRow = new EventBean[2];
                    resultRow[_myStreamNumber] = lookupEvents[count];
                    joinSet.Add(new MultiKeyArrayOfKeys<EventBean>(resultRow));
                }
                else {
                    bool foundMatch = false;
                    if (subsetIterAdvanced) {
                        // Add each row to the join result or, for outer joins, run through the outer join filter

                        do {
                            var resultRow = new EventBean[2];
                            resultRow[_myStreamNumber] = lookupEvents[count];
                            resultRow[_historicalStreamNumber] = subsetIter.Current;

                            // In an outer join compare the on-fields
                            if (_outerJoinCompareNode != null) {
                                var compareResult = _outerJoinCompareNode.Evaluate(
                                    resultRow,
                                    true,
                                    exprEvaluatorContext);
                                if ((compareResult != null) && true.Equals(compareResult)) {
                                    joinSet.Add(new MultiKeyArrayOfKeys<EventBean>(resultRow));
                                    foundMatch = true;
                                }
                            }
                            else {
                                joinSet.Add(new MultiKeyArrayOfKeys<EventBean>(resultRow));
                            }
                        } while (subsetIter.MoveNext());
                    }

                    if ((_isOuterJoin) && (!foundMatch)) {
                        var resultRow = new EventBean[2];
                        resultRow[_myStreamNumber] = lookupEvents[count];
                        joinSet.Add(new MultiKeyArrayOfKeys<EventBean>(resultRow));
                    }
                }

                count++;
            }
        }

        #endregion QueryStrategy Members
    }
}