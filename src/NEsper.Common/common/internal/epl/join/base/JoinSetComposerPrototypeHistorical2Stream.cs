///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.historical.lookupstrategy;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.join.@base
{
    public class JoinSetComposerPrototypeHistorical2Stream : JoinSetComposerPrototypeBase
    {
        private PollResultIndexingStrategy indexingStrategy;
        private bool isAllHistoricalNoSubordinate;
        private HistoricalIndexLookupStrategy lookupStrategy;
        private ExprEvaluator outerJoinEqualsEval;
        private bool[] outerJoinPerStream;

        private int polledNum;
        private int streamNum;

        public int PolledNum {
            set => polledNum = value;
        }

        public int StreamNum {
            set => streamNum = value;
        }

        public ExprEvaluator OuterJoinEqualsEval {
            set => outerJoinEqualsEval = value;
        }

        public HistoricalIndexLookupStrategy LookupStrategy {
            set => lookupStrategy = value;
        }

        public PollResultIndexingStrategy IndexingStrategy {
            set => indexingStrategy = value;
        }

        public bool IsAllHistoricalNoSubordinate {
            set => isAllHistoricalNoSubordinate = value;
        }

        public bool[] IsOuterJoinPerStream {
            set => outerJoinPerStream = value;
        }

        public override JoinSetComposerDesc Create(
            Viewable[] streamViews,
            bool isFireAndForget,
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var queryStrategies = new QueryStrategy[streamTypes.Length];

            var viewable = (HistoricalEventViewable)streamViews[polledNum];
            queryStrategies[streamNum] = new HistoricalDataQueryStrategy(
                streamNum,
                polledNum,
                viewable,
                outerJoinPerStream[streamNum],
                outerJoinEqualsEval,
                lookupStrategy,
                indexingStrategy);

            // for strictly historical joins, create a query strategy for the non-subordinate historical view
            if (isAllHistoricalNoSubordinate) {
                viewable = (HistoricalEventViewable)streamViews[streamNum];
                queryStrategies[polledNum] = new HistoricalDataQueryStrategy(
                    polledNum,
                    streamNum,
                    viewable,
                    outerJoinPerStream[polledNum],
                    outerJoinEqualsEval,
                    HistoricalIndexLookupStrategyNoIndex.INSTANCE,
                    PollResultIndexingStrategyNoIndex.INSTANCE);
            }

            var allowIndexInit = agentInstanceContext.EventTableIndexService.AllowInitIndex(isRecoveringResilient);
            JoinSetComposer composer = new JoinSetComposerHistoricalImpl(
                allowIndexInit,
                null,
                queryStrategies,
                streamViews,
                agentInstanceContext);
            return new JoinSetComposerDesc(composer, postJoinFilterEvaluator);
        }
    }
} // end of namespace