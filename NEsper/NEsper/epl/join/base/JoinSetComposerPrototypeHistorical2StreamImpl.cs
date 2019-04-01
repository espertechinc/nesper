///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.epl.spec;
using com.espertech.esper.type;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.join.@base
{
    public class JoinSetComposerPrototypeHistorical2StreamImpl : JoinSetComposerPrototype
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly ExprNode _optionalFilterNode;
        private readonly EventType[] _streamTypes;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly int _polledViewNum;
        private readonly int _streamViewNum;
        private readonly bool _isOuterJoin;
        private readonly ExprNode _outerJoinEqualsNode;
        private readonly Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> _indexStrategies;
        private readonly bool _isAllHistoricalNoSubordinate;
        private readonly OuterJoinDesc[] _outerJoinDescList;
        private readonly bool _allowIndexInit;

        public JoinSetComposerPrototypeHistorical2StreamImpl(
            ExprNode optionalFilterNode,
            EventType[] streamTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            int polledViewNum,
            int streamViewNum,
            bool outerJoin,
            ExprNode outerJoinEqualsNode,
            Pair<HistoricalIndexLookupStrategy, PollResultIndexingStrategy> indexStrategies,
            bool allHistoricalNoSubordinate,
            OuterJoinDesc[] outerJoinDescList,
            bool allowIndexInit)
        {
            _optionalFilterNode = optionalFilterNode;
            _streamTypes = streamTypes;
            _exprEvaluatorContext = exprEvaluatorContext;
            _polledViewNum = polledViewNum;
            _streamViewNum = streamViewNum;
            _isOuterJoin = outerJoin;
            _outerJoinEqualsNode = outerJoinEqualsNode;
            _indexStrategies = indexStrategies;
            _isAllHistoricalNoSubordinate = allHistoricalNoSubordinate;
            _outerJoinDescList = outerJoinDescList;
            _allowIndexInit = allowIndexInit;
        }
    
        public JoinSetComposerDesc Create(Viewable[] streamViews, bool isFireAndForget, AgentInstanceContext agentInstanceContext, bool isRecoveringResilient) {
            var queryStrategies = new QueryStrategy[_streamTypes.Length];
            var viewable = (HistoricalEventViewable) streamViews[_polledViewNum];
            var outerJoinEqualsNodeEval = _outerJoinEqualsNode == null ? null : _outerJoinEqualsNode.ExprEvaluator;
            queryStrategies[_streamViewNum] = new HistoricalDataQueryStrategy(_streamViewNum, _polledViewNum, viewable, _isOuterJoin, outerJoinEqualsNodeEval,
                    _indexStrategies.First, _indexStrategies.Second);
    
            // for strictly historical joins, create a query strategy for the non-subordinate historical view
            if (_isAllHistoricalNoSubordinate) {
                var isOuterJoinX = false;
                if (_outerJoinDescList.Length > 0) {
                    var outerJoinDesc = _outerJoinDescList[0];
                    if (outerJoinDesc.OuterJoinType.Equals(OuterJoinType.FULL)) {
                        isOuterJoinX = true;
                    } else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.LEFT)) &&
                            (_polledViewNum == 0)) {
                        isOuterJoinX = true;
                    } else if ((outerJoinDesc.OuterJoinType.Equals(OuterJoinType.RIGHT)) &&
                            (_polledViewNum == 1)) {
                        isOuterJoinX = true;
                    }
                }
                viewable = (HistoricalEventViewable) streamViews[_streamViewNum];
                queryStrategies[_polledViewNum] = new HistoricalDataQueryStrategy(_polledViewNum, _streamViewNum, viewable, isOuterJoinX, outerJoinEqualsNodeEval,
                        new HistoricalIndexLookupStrategyNoIndex(), new PollResultIndexingStrategyNoIndex());
            }
    
            var composer = new JoinSetComposerHistoricalImpl(_allowIndexInit, null, queryStrategies, streamViews, _exprEvaluatorContext);
            var postJoinEval = _optionalFilterNode == null ? null : _optionalFilterNode.ExprEvaluator;
            return new JoinSetComposerDesc(composer, postJoinEval);
        }
    }
} // end of namespace
