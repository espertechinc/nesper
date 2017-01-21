///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result-set processor _prototype for the aggregate-grouped case: there is a group-by and 
    /// one or more non-aggregation event properties in the select clause are not listed in the 
    /// group by, and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorAggregateGroupedFactory : ResultSetProcessorFactory
    {
        private readonly SelectExprProcessor _selectExprProcessor;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectExprProcessor">for processing the select expression and generting the readonly output rows</param>
        /// <param name="groupKeyNodeExpressions">The group key node expressions.</param>
        /// <param name="groupKeyNodes">list of group-by expression nodes needed for building the group-by keys</param>
        /// <param name="optionalHavingNode">expression node representing validated HAVING clause, or null if none given.Aggregation functions in the having node must have been pointed to the AggregationService for evaluation.</param>
        /// <param name="isSelectRStream">true if remove stream events should be generated</param>
        /// <param name="isUnidirectional">true if unidirectional join</param>
        /// <param name="outputLimitSpec">The output limit spec.</param>
        /// <param name="isSorting">if set to <c>true</c> [is sorting].</param>
        /// <param name="isHistoricalOnly">if set to <c>true</c> [is historical only].</param>
        /// <param name="resultSetProcessorHelperFactory">The result set processor helper factory.</param>
        /// <param name="optionalOutputFirstConditionFactory">The optional output first condition factory.</param>
        /// <param name="enableOutputLimitOpt">if set to <c>true</c> [enable output limit opt].</param>
        /// <param name="numStreams">The number streams.</param>
        public ResultSetProcessorAggregateGroupedFactory(
            SelectExprProcessor selectExprProcessor,
            ExprNode[] groupKeyNodeExpressions,
            ExprEvaluator[] groupKeyNodes,
            ExprEvaluator optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool isHistoricalOnly,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            bool enableOutputLimitOpt,
            int numStreams)
        {
            _selectExprProcessor = selectExprProcessor;
            GroupKeyNodeExpressions = groupKeyNodeExpressions;
            GroupKeyNode = groupKeyNodes.Length == 1 ? groupKeyNodes[0] : null;
            GroupKeyNodes = groupKeyNodes;
            OptionalHavingNode = optionalHavingNode;
            IsSorting = isSorting;
            IsSelectRStream = isSelectRStream;
            IsUnidirectional = isUnidirectional;
            IsHistoricalOnly = isHistoricalOnly;
            OutputLimitSpec = outputLimitSpec;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            OptionalOutputFirstConditionFactory = optionalOutputFirstConditionFactory;
            IsEnableOutputLimitOpt = enableOutputLimitOpt;
            NumStreams = numStreams;
        }
    
        public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorAggregateGrouped(this, _selectExprProcessor, orderByProcessor, aggregationService, agentInstanceContext);
        }

        public EventType ResultEventType
        {
            get { return _selectExprProcessor.ResultEventType; }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public ExprEvaluator[] GroupKeyNodes { get; private set; }

        public ExprEvaluator OptionalHavingNode { get; private set; }

        public bool IsSorting { get; private set; }

        public bool IsSelectRStream { get; private set; }

        public bool IsUnidirectional { get; private set; }

        public bool IsHistoricalOnly { get; private set; }

        public OutputLimitSpec OutputLimitSpec { get; private set; }

        public ExprEvaluator GroupKeyNode { get; private set; }

        public ExprNode[] GroupKeyNodeExpressions { get; private set; }

        public bool IsOutputLast
        {
            get { return OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST; }
        }

        public bool IsOutputAll
        {
            get { return OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL; }
        }

        public ResultSetProcessorType ResultSetProcessorType
        {
            get { return ResultSetProcessorType.AGGREGATED_GROUPED; }
        }

        public OutputConditionPolledFactory OptionalOutputFirstConditionFactory { get; private set; }

        public bool IsEnableOutputLimitOpt { get; private set; }

        public int NumStreams { get; private set; }

        public bool IsOutputFirst
        {
            get { return OutputLimitSpec != null && OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST; }
        }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; private set; }
    }
}
