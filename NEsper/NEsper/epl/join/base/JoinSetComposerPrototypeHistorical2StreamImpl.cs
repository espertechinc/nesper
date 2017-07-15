///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.pollindex;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.@base
{
    public class JoinSetComposerPrototypeHistorical2StreamImpl : JoinSetComposerPrototype {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ExprNode optionalFilterNode;
        private readonly EventType[] streamTypes;
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly int polledViewNum;
        private readonly int streamViewNum;
        private readonly bool isOuterJoin;
        private readonly ExprNode outerJoinEqualsNode;
        private readonly Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> indexStrategies;
        private readonly bool isAllHistoricalNoSubordinate;
        private readonly OuterJoinDesc[] outerJoinDescList;
        private readonly bool allowIndexInit;
    
        public JoinSetComposerPrototypeHistorical2StreamImpl(ExprNode optionalFilterNode, EventType[] streamTypes, ExprEvaluatorContext exprEvaluatorContext, int polledViewNum, int streamViewNum, bool outerJoin, ExprNode outerJoinEqualsNode, Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> indexStrategies, bool allHistoricalNoSubordinate, OuterJoinDesc[] outerJoinDescList, bool allowIndexInit) {
            this.optionalFilterNode = optionalFilterNode;
            this.streamTypes = streamTypes;
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.polledViewNum = polledViewNum;
            this.streamViewNum = streamViewNum;
            isOuterJoin = outerJoin;
            this.outerJoinEqualsNode = outerJoinEqualsNode;
            this.indexStrategies = indexStrategies;
            isAllHistoricalNoSubordinate = allHistoricalNoSubordinate;
            this.outerJoinDescList = outerJoinDescList;
            this.allowIndexInit = allowIndexInit;
        }
    
        public JoinSetComposerDesc Create(Viewable[] streamViews, bool isFireAndForget, AgentInstanceContext agentInstanceContext, bool isRecoveringResilient) {
            var queryStrategies = new QueryStrategy[streamTypes.Length];
    
            HistoricalEventViewable viewable = (HistoricalEventViewable) streamViews[polledViewNum];
            ExprEvaluator outerJoinEqualsNodeEval = outerJoinEqualsNode == null ? null : outerJoinEqualsNode.ExprEvaluator;
            queryStrategies[streamViewNum] = new HistoricalDataQueryStrategy(streamViewNum, polledViewNum, viewable, isOuterJoin, outerJoinEqualsNodeEval,
                    indexStrategies.First, indexStrategies.Second);
    
            // for strictly historical joins, create a query strategy for the non-subordinate historical view
            if (isAllHistoricalNoSubordinate) {
                bool isOuterJoin = false;
                if (outerJoinDescList.Length > 0) {
                    OuterJoinDesc outerJoinDesc = outerJoinDescList[0];
                    if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.FULL)) {
                        isOuterJoin = true;
                    } else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.LEFT)) &&
                            (polledViewNum == 0)) {
                        isOuterJoin = true;
                    } else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.RIGHT)) &&
                            (polledViewNum == 1)) {
                        isOuterJoin = true;
                    }
                }
                viewable = (HistoricalEventViewable) streamViews[streamViewNum];
                queryStrategies[polledViewNum] = new HistoricalDataQueryStrategy(polledViewNum, streamViewNum, viewable, isOuterJoin, outerJoinEqualsNodeEval,
                        new HistoricalIndexLookupStrategyNoIndex(), new PollResultIndexingStrategyNoIndex());
            }
    
            var composer = new JoinSetComposerHistoricalImpl(allowIndexInit, null, queryStrategies, streamViews, exprEvaluatorContext);
            ExprEvaluator postJoinEval = optionalFilterNode == null ? null : optionalFilterNode.ExprEvaluator;
            return new JoinSetComposerDesc(composer, postJoinEval);
        }
    }
} // end of namespace
