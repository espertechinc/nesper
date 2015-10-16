///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor Prototype for the fully-grouped case:
    /// there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
    /// and there are aggregation functions.
    /// </summary>
    public class ResultSetProcessorRowPerGroupFactory : ResultSetProcessorFactory
    {
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly ExprNode[] _groupKeyNodeExpressions;
        private readonly ExprEvaluator _groupKeyNode;
        private readonly ExprEvaluator[] _groupKeyNodes;
        private readonly ExprEvaluator _optionalHavingNode;
        private readonly bool _isSorting;
        private readonly bool _isSelectRStream;
        private readonly bool _isUnidirectional;
        private readonly OutputLimitSpec _outputLimitSpec;
        private readonly bool _noDataWindowSingleSnapshot;
        private readonly bool _isHistoricalOnly;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectExprProcessor">for processing the select expression and generting the final output rows</param>
        /// <param name="groupKeyNodeExpressions">The group key node expressions.</param>
        /// <param name="groupKeyNodes">list of group-by expression nodes needed for building the group-by keys</param>
        /// <param name="optionalHavingNode">expression node representing validated HAVING clause, or null if none given.Aggregation functions in the having node must have been pointed to the AggregationService for evaluation.</param>
        /// <param name="isSelectRStream">true if remove stream events should be generated</param>
        /// <param name="isUnidirectional">true if unidirectional join</param>
        /// <param name="outputLimitSpec">The output limit spec.</param>
        /// <param name="isSorting">if set to <c>true</c> [is sorting].</param>
        /// <param name="noDataWindowSingleStream">if set to <c>true</c> [no data window single stream].</param>
        /// <param name="isHistoricalOnly">if set to <c>true</c> [is historical only].</param>
        public ResultSetProcessorRowPerGroupFactory(
            SelectExprProcessor selectExprProcessor,
            ExprNode[] groupKeyNodeExpressions,
            ExprEvaluator[] groupKeyNodes,
            ExprEvaluator optionalHavingNode,
            bool isSelectRStream,
            bool isUnidirectional,
            OutputLimitSpec outputLimitSpec,
            bool isSorting,
            bool noDataWindowSingleStream,
            bool isHistoricalOnly,
            bool iterateUnbounded)
        {
            _groupKeyNodeExpressions = groupKeyNodeExpressions;
            _selectExprProcessor = selectExprProcessor;
            _groupKeyNodes = groupKeyNodes;
            _groupKeyNode = groupKeyNodes.Length == 1 ? groupKeyNodes[0] : null;
            _optionalHavingNode = optionalHavingNode;
            _isSorting = isSorting;
            _isSelectRStream = isSelectRStream;
            _isUnidirectional = isUnidirectional;
            _outputLimitSpec = outputLimitSpec;
            _noDataWindowSingleSnapshot = iterateUnbounded || (outputLimitSpec != null && outputLimitSpec.DisplayLimit == OutputLimitLimitType.SNAPSHOT && noDataWindowSingleStream);
            _isHistoricalOnly = isHistoricalOnly;
        }

        public ResultSetProcessor Instantiate(
            OrderByProcessor orderByProcessor,
            AggregationService aggregationService,
            AgentInstanceContext agentInstanceContext)
        {
            if (_noDataWindowSingleSnapshot && !_isHistoricalOnly) {
                return new ResultSetProcessorRowPerGroupUnbound(
                    this, _selectExprProcessor, orderByProcessor, aggregationService, agentInstanceContext);
            }
            return new ResultSetProcessorRowPerGroup(
                this, _selectExprProcessor, orderByProcessor, aggregationService, agentInstanceContext);
        }

        public EventType ResultEventType
        {
            get { return _selectExprProcessor.ResultEventType; }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public ExprEvaluator[] GroupKeyNodes
        {
            get { return _groupKeyNodes; }
        }

        public ExprEvaluator GroupKeyNode
        {
            get { return _groupKeyNode; }
        }

        public ExprEvaluator OptionalHavingNode
        {
            get { return _optionalHavingNode; }
        }

        public bool IsSorting
        {
            get { return _isSorting; }
        }

        public bool IsSelectRStream
        {
            get { return _isSelectRStream; }
        }

        public bool IsUnidirectional
        {
            get { return _isUnidirectional; }
        }

        public OutputLimitSpec OutputLimitSpec
        {
            get { return _outputLimitSpec; }
        }

        public ExprNode[] GroupKeyNodeExpressions
        {
            get { return _groupKeyNodeExpressions; }
        }

        public bool IsHistoricalOnly
        {
            get { return _isHistoricalOnly; }
        }

        public ResultSetProcessorType ResultSetProcessorType
        {
            get { return ResultSetProcessorType.FULLYAGGREGATED_GROUPED; }
        }

        public bool IsOutputLast
        {
            get { return _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST; }
        }

        public bool IsOutputAll
        {
            get { return _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL; }
        }
    }
}
