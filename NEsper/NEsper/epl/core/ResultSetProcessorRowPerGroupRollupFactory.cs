///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor _prototype for the fully-grouped case: there is a group-by and 
    /// all non-aggregation event properties in the select clause are listed in the group by, 
    /// and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupRollupFactory : ResultSetProcessorFactory
    {
        private readonly bool _noDataWindowSingleSnapshot;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="perLevelExpression">The per level expression.</param>
        /// <param name="groupKeyNodeExpressions">The group key node expressions.</param>
        /// <param name="groupKeyNodes">list of group-by expression nodes needed for building the group-by keysAggregation functions in the having node must have been pointed to the AggregationService for evaluation.</param>
        /// <param name="isSelectRStream">true if remove stream events should be generated</param>
        /// <param name="isUnidirectional">true if unidirectional join</param>
        /// <param name="outputLimitSpec">The output limit spec.</param>
        /// <param name="isSorting">if set to <c>true</c> [is sorting].</param>
        /// <param name="noDataWindowSingleStream">if set to <c>true</c> [no data window single stream].</param>
        /// <param name="groupByRollupDesc">The group by rollup desc.</param>
        /// <param name="isJoin">if set to <c>true</c> [is join].</param>
        /// <param name="isHistoricalOnly">if set to <c>true</c> [is historical only].</param>
        /// <param name="iterateUnbounded">if set to <c>true</c> [iterate unbounded].</param>
        /// <param name="optionalOutputFirstConditionFactory">The optional output first condition factory.</param>
        /// <param name="resultSetProcessorHelperFactory">The result set processor helper factory.</param>
        /// <param name="enableOutputLimitOpt">if set to <c>true</c> [enable output limit opt].</param>
        /// <param name="numStreams">The number streams.</param>
        public ResultSetProcessorRowPerGroupRollupFactory(
            GroupByRollupPerLevelExpression perLevelExpression,
            ExprNode[] groupKeyNodeExpressions,
            ExprEvaluator[] groupKeyNodes,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool noDataWindowSingleStream,
            AggregationGroupByRollupDesc groupByRollupDesc,
            bool isJoin,
            bool isHistoricalOnly,
            bool iterateUnbounded,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            bool enableOutputLimitOpt,
            int numStreams)
        {
            GroupKeyNodeExpressions = groupKeyNodeExpressions;
            PerLevelExpression = perLevelExpression;
            GroupKeyNodes = groupKeyNodes;
            GroupKeyNode = groupKeyNodes.Length == 1 ? groupKeyNodes[0] : null;
            IsSorting = isSorting;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            OutputLimitSpec = outputLimitSpec;
            _noDataWindowSingleSnapshot = iterateUnbounded || (outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT && noDataWindowSingleStream);
            GroupByRollupDesc = groupByRollupDesc;
            IsJoin = isJoin;
            IsHistoricalOnly = isHistoricalOnly;
            OptionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            IsEnableOutputLimitOpt = enableOutputLimitOpt;
            NumStreams = numStreams;
        }
    
        public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            if (_noDataWindowSingleSnapshot && !IsHistoricalOnly) {
                return new ResultSetProcessorRowPerGroupRollupUnbound(this, orderByProcessor, aggregationService, agentInstanceContext);
            }
            return new ResultSetProcessorRowPerGroupRollup(this, orderByProcessor, aggregationService, agentInstanceContext);
        }

        public EventType ResultEventType
        {
            get { return PerLevelExpression.SelectExprProcessor[0].ResultEventType; }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public ExprEvaluator[] GroupKeyNodes { get; private set; }

        public ExprEvaluator GroupKeyNode { get; private set; }

        public bool IsSorting { get; private set; }

        public bool IsSelectRStream { get; private set; }

        public bool IsUnidirectional { get; private set; }

        public OutputLimitSpec OutputLimitSpec { get; private set; }

        public ExprNode[] GroupKeyNodeExpressions { get; private set; }

        public AggregationGroupByRollupDesc GroupByRollupDesc { get; private set; }

        public GroupByRollupPerLevelExpression PerLevelExpression { get; private set; }

        public bool IsJoin { get; private set; }

        public bool IsHistoricalOnly { get; private set; }

        public ResultSetProcessorType ResultSetProcessorType
        {
            get { return ResultSetProcessorType.FULLYAGGREGATED_GROUPED_ROLLUP; }
        }

        public OutputConditionPolledFactory OptionalOutputFirstConditionFactory { get; private set; }

        public bool IsEnableOutputLimitOpt { get; private set; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; private set; }

        public int NumStreams { get; private set; }
    }
}
