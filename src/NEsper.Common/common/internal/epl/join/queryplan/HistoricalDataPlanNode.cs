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
        private PollResultIndexingStrategy indexingStrategy;
        private HistoricalIndexLookupStrategy lookupStrategy;
        private int numStreams = -1;
        private ExprEvaluator outerJoinExprEval;
        private int rootStreamNum = -1;
        private int streamNum = -1;

        public int StreamNum {
            set => streamNum = value;
        }

        public int NumStreams {
            set => numStreams = value;
        }

        public HistoricalIndexLookupStrategy LookupStrategy {
            set => lookupStrategy = value;
        }

        public PollResultIndexingStrategy IndexingStrategy {
            set => indexingStrategy = value;
        }

        public int RootStreamNum {
            set => rootStreamNum = value;
        }

        public ExprEvaluator OuterJoinExprEval {
            set => outerJoinExprEval = value;
        }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            var viewable = (HistoricalEventViewable) streamViews[streamNum];
            return new HistoricalDataExecNode(viewable, indexingStrategy, lookupStrategy, numStreams, streamNum);
        }

        /// <summary>
        ///     Returns the table lookup strategy for use in outer joins.
        /// </summary>
        /// <param name="streamViews">all views in join</param>
        /// <returns>strategy</returns>
        public HistoricalTableLookupStrategy MakeOuterJoinStategy(Viewable[] streamViews)
        {
            var viewable = (HistoricalEventViewable) streamViews[streamNum];
            return new HistoricalTableLookupStrategy(
                viewable,
                indexingStrategy,
                lookupStrategy,
                numStreams,
                streamNum,
                rootStreamNum,
                outerJoinExprEval);
        }

        public void AddIndexes(HashSet<TableLookupIndexReqKey> usedIndexes)
        {
            // none to add
        }

        protected void Print(IndentWriter writer)
        {
            writer.IncrIndent();
            writer.WriteLine("HistoricalDataPlanNode streamNum=" + streamNum);
        }
    }
} // end of namespace