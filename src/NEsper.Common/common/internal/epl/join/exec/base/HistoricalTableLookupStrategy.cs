///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.join.exec.@base
{
    /// <summary>
    ///     A lookup strategy for use in outer joins onto historical streams.
    /// </summary>
    public class HistoricalTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly PollResultIndexingStrategy indexingStrategy;
        private readonly EventBean[][] lookupEventsPerStream;
        private readonly HistoricalIndexLookupStrategy lookupStrategy;
        private readonly ExprEvaluator outerJoinExprNode;
        private readonly int rootStreamNum;
        private readonly int streamNum;
        private readonly HistoricalEventViewable viewable;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="viewable">providing the polling access</param>
        /// <param name="indexingStrategy">strategy for indexing results</param>
        /// <param name="lookupStrategy">strategy for using indexed results</param>
        /// <param name="numStreams">number of streams</param>
        /// <param name="streamNum">stream number of the historical stream</param>
        /// <param name="rootStreamNum">the query plan root stream number</param>
        /// <param name="outerJoinExprNode">an optional outer join expression</param>
        public HistoricalTableLookupStrategy(
            HistoricalEventViewable viewable,
            PollResultIndexingStrategy indexingStrategy,
            HistoricalIndexLookupStrategy lookupStrategy,
            int numStreams,
            int streamNum,
            int rootStreamNum,
            ExprEvaluator outerJoinExprNode)
        {
            this.viewable = viewable;
            this.indexingStrategy = indexingStrategy;
            this.lookupStrategy = lookupStrategy;
            this.streamNum = streamNum;
            this.rootStreamNum = rootStreamNum;
            this.outerJoinExprNode = outerJoinExprNode;
            lookupEventsPerStream = new[] {
                new EventBean[numStreams]
            };
        }

        public LookupStrategyDesc StrategyDesc => null;

        public LookupStrategyType LookupStrategyType => LookupStrategyType.HISTORICAL;

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QIndexJoinLookup(this, null);

            int currStream = cursor.Stream;

            // fill the current stream and the deep cursor events
            lookupEventsPerStream[0][currStream] = theEvent;
            RecursiveFill(lookupEventsPerStream[0], cursor.Node);

            // poll
            var indexPerLookupRow = viewable.Poll(lookupEventsPerStream, indexingStrategy, exprEvaluatorContext);

            ISet<EventBean> result = null;
            foreach (var index in indexPerLookupRow) {
                // Using the index, determine a subset of the whole indexed table to process, unless
                // the strategy is a full table scan
                var subsetIter = lookupStrategy.Lookup(theEvent, index, exprEvaluatorContext);

                if (subsetIter != null) {
                    if (outerJoinExprNode != null) {
                        // Add each row to the join result or, for outer joins, run through the outer join filter
                        for (; subsetIter.MoveNext();) {
                            var candidate = subsetIter.Current;

                            lookupEventsPerStream[0][streamNum] = candidate;
                            var pass = outerJoinExprNode.Evaluate(lookupEventsPerStream[0], true, exprEvaluatorContext);
                            if (pass != null && true.Equals(pass)) {
                                if (result == null) {
                                    result = new HashSet<EventBean>();
                                }

                                result.Add(candidate);
                            }
                        }
                    }
                    else {
                        // Add each row to the join result or, for outer joins, run through the outer join filter
                        for (; subsetIter.MoveNext();) {
                            var candidate = subsetIter.Current;
                            if (result == null) {
                                result = new HashSet<EventBean>();
                            }

                            result.Add(candidate);
                        }
                    }
                }
            }

            instrumentationCommon.AIndexJoinLookup(result, null);
            return result;
        }

        private void RecursiveFill(
            EventBean[] lookupEventsPerStream,
            Node node)
        {
            if (node == null) {
                return;
            }

            var parent = node.Parent;
            if (parent == null) {
                lookupEventsPerStream[rootStreamNum] = node.ParentEvent;
                return;
            }

            lookupEventsPerStream[parent.Stream] = node.ParentEvent;
            RecursiveFill(lookupEventsPerStream, parent);
        }
    }
} // end of namespace