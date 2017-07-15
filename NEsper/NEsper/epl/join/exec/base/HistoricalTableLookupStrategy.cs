///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.@base;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.epl.@join.rep;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.exec.@base
{
    /// <summary>
    /// A lookup strategy for use in outer joins onto historical streams.
    /// </summary>
    public class HistoricalTableLookupStrategy : JoinExecTableLookupStrategy
    {
        private readonly HistoricalEventViewable _viewable;
        private readonly PollResultIndexingStrategy _indexingStrategy;
        private readonly HistoricalIndexLookupStrategy _lookupStrategy;
        private readonly int _streamNum;
        private readonly int _rootStreamNum;
        private readonly ExprEvaluator _outerJoinExprNode;
        private readonly EventBean[][] _lookupEventsPerStream;

        /// <summary>
        /// Ctor.
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
            _viewable = viewable;
            _indexingStrategy = indexingStrategy;
            _lookupStrategy = lookupStrategy;
            _streamNum = streamNum;
            _rootStreamNum = rootStreamNum;
            _outerJoinExprNode = outerJoinExprNode;
            _lookupEventsPerStream = new EventBean[1][numStreams];
        }

        public ICollection<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext)
        {
            int currStream = cursor.Stream;

            // fill the current stream and the deep cursor events
            _lookupEventsPerStream[0][currStream] = theEvent;
            RecursiveFill(_lookupEventsPerStream[0], cursor.Node);

            // poll
            EventTable[][] indexPerLookupRow = _viewable.Poll(
                _lookupEventsPerStream, _indexingStrategy, exprEvaluatorContext);

            ISet<EventBean> result = null;
            foreach (EventTable[] index in indexPerLookupRow)
            {
                // Using the index, determine a subset of the whole indexed table to process, unless
                // the strategy is a full table scan
                IEnumerator<EventBean> subsetIter = _lookupStrategy.Lookup(theEvent, index, exprEvaluatorContext);

                if (subsetIter != null)
                {
                    if (_outerJoinExprNode != null)
                    {
                        // Add each row to the join result or, for outer joins, run through the outer join filter
                        for (; subsetIter.HasNext();)
                        {
                            EventBean candidate = subsetIter.Next();

                            _lookupEventsPerStream[0][_streamNum] = candidate;
                            bool? pass = (bool?) _outerJoinExprNode.Evaluate(_lookupEventsPerStream[0], true, exprEvaluatorContext);
                            if ((pass != null) && pass)
                            {
                                if (result == null)
                                {
                                    result = new HashSet<EventBean>();
                                }
                                result.Add(candidate);
                            }
                        }
                    }
                    else
                    {
                        // Add each row to the join result or, for outer joins, run through the outer join filter
                        for (; subsetIter.HasNext();)
                        {
                            EventBean candidate = subsetIter.Next();
                            if (result == null)
                            {
                                result = new HashSet<EventBean>();
                            }
                            result.Add(candidate);
                        }
                    }
                }
            }

            return result;
        }

        private void RecursiveFill(EventBean[] lookupEventsPerStream, Node node)
        {
            if (node == null)
            {
                return;
            }

            Node parent = node.Parent;
            if (parent == null)
            {
                lookupEventsPerStream[_rootStreamNum] = node.ParentEvent;
                return;
            }

            lookupEventsPerStream[parent.Stream] = node.ParentEvent;
            RecursiveFill(lookupEventsPerStream, parent);
        }

        public LookupStrategyDesc StrategyDesc
        {
            get { return null; }
        }
    }
} // end of namespace
