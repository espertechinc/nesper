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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor _prototype for the simplest case: no aggregation functions used in the select clause, and no group-by.
    /// </summary>
    public class ResultSetProcessorSimpleFactory : ResultSetProcessorFactory
    {
        private readonly bool _isSelectRStream;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly ExprEvaluator _optionalHavingExpr;
        private readonly OutputLimitSpec _outputLimitSpec;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="selectExprProcessor">for processing the select expression and generting the readonly output rows</param>
        /// <param name="optionalHavingNode">having clause expression node</param>
        /// <param name="isSelectRStream">true if remove stream events should be generated</param>
        /// <param name="outputLimitSpec">The output limit spec.</param>
        /// <param name="enableOutputLimitOpt">if set to <c>true</c> [enable output limit opt].</param>
        /// <param name="resultSetProcessorHelperFactory">The result set processor helper factory.</param>
        /// <param name="numStreams">The number streams.</param>
        public ResultSetProcessorSimpleFactory(
            SelectExprProcessor selectExprProcessor,
            ExprEvaluator optionalHavingNode,
            bool isSelectRStream,
            OutputLimitSpec outputLimitSpec,
            bool enableOutputLimitOpt,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            int numStreams)
        {
            _selectExprProcessor = selectExprProcessor;
            _optionalHavingExpr = optionalHavingNode;
            _isSelectRStream = isSelectRStream;
            _outputLimitSpec = outputLimitSpec;
            IsEnableOutputLimitOpt = enableOutputLimitOpt;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            NumStreams = numStreams;
        }

        public ResultSetProcessorType ResultSetProcessorType
        {
            get { return ResultSetProcessorType.UNAGGREGATED_UNGROUPED; }
        }

        public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorSimple(this, _selectExprProcessor, orderByProcessor, agentInstanceContext);
        }

        public EventType ResultEventType
        {
            get { return _selectExprProcessor.ResultEventType; }
        }

        public bool HasAggregation
        {
            get { return false; }
        }

        public bool IsSelectRStream
        {
            get { return _isSelectRStream; }
        }

        public ExprEvaluator OptionalHavingExpr
        {
            get { return _optionalHavingExpr; }
        }

        public bool IsOutputLast
        {
            get { return _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST; }
        }

        public bool IsOutputAll
        {
            get { return _outputLimitSpec != null && _outputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL; }
        }

        public bool IsEnableOutputLimitOpt { get; private set; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; private set; }

        public int NumStreams { get; private set; }
    }
}
