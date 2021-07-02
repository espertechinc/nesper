///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Query plan for performing a historical data lookup.
    ///     <para />
    ///     Translates into a particular execution for use in regular and outer joins.
    /// </summary>
    public class HistoricalDataPlanNode : QueryPlanNode
    {
        private PollResultIndexingStrategy _indexingStrategy;
        private HistoricalIndexLookupStrategy _lookupStrategy;
        private int _numStreams = -1;
        private ExprEvaluator _outerJoinExprEval;
        private int _rootStreamNum = -1;
        private int _streamNum = -1;

        public int StreamNum {
            set => _streamNum = value;
        }

        public int NumStreams {
            set => _numStreams = value;
        }

        public HistoricalIndexLookupStrategy LookupStrategy {
            set => _lookupStrategy = value;
        }

        public PollResultIndexingStrategy IndexingStrategy {
            set => _indexingStrategy = value;
        }

        public int RootStreamNum {
            set => _rootStreamNum = value;
        }

        public ExprEvaluator OuterJoinExprEval {
            set => _outerJoinExprEval = value;
        }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            var viewable = (HistoricalEventViewable) streamViews[_streamNum];
            return new HistoricalDataExecNode(viewable, _indexingStrategy, _lookupStrategy, _numStreams, _streamNum);
        }

        /// <summary>
        ///     Returns the table lookup strategy for use in outer joins.
        /// </summary>
        /// <param name="streamViews">all views in join</param>
        /// <returns>strategy</returns>
        public HistoricalTableLookupStrategy MakeOuterJoinStategy(Viewable[] streamViews)
        {
            var viewable = (HistoricalEventViewable) streamViews[_streamNum];
            return new HistoricalTableLookupStrategy(
                viewable,
                _indexingStrategy,
                _lookupStrategy,
                _numStreams,
                _streamNum,
                _rootStreamNum,
                _outerJoinExprEval);
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            // none to add
        }

        protected void Print(IndentWriter writer)
        {
            writer.IncrIndent();
            writer.WriteLine("HistoricalDataPlanNode streamNum=" + _streamNum);
        }
    }
} // end of namespace